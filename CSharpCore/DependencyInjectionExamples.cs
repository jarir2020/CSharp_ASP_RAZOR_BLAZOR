using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpCore;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
}

public interface IGreetingFormatter
{
    string Format(string name);
}

public sealed class GreetingFormatter : IGreetingFormatter
{
    public string Format(string name)
    {
        return name.Trim();
    }
}

/// <summary>
/// A service receives its dependencies through its constructor. It does not
/// create a clock or formatter internally, so both can be replaced in tests.
/// </summary>
public sealed class GreetingService
{
    private readonly IClock _clock;
    private readonly IGreetingFormatter _formatter;

    public GreetingService(IClock clock, IGreetingFormatter formatter)
    {
        _clock = clock;
        _formatter = formatter;
    }

    public string CreateGreeting(string name)
    {
        string formattedName = _formatter.Format(name);
        string timestamp = _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        return $"Hello, {formattedName}! UTC: {timestamp}";
    }
}

public sealed class RequestContext
{
    public Guid RequestId { get; } = Guid.NewGuid();
}

public sealed class TransientMarker
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}

public sealed class LifetimeObservation
{
    public LifetimeObservation(
        bool singletonIsShared,
        bool transientIsNewEachTime,
        bool scopedIsSharedWithinScope,
        bool scopedIsNewForAnotherScope)
    {
        SingletonIsShared = singletonIsShared;
        TransientIsNewEachTime = transientIsNewEachTime;
        ScopedIsSharedWithinScope = scopedIsSharedWithinScope;
        ScopedIsNewForAnotherScope = scopedIsNewForAnotherScope;
    }

    public bool SingletonIsShared { get; }

    public bool TransientIsNewEachTime { get; }

    public bool ScopedIsSharedWithinScope { get; }

    public bool ScopedIsNewForAnotherScope { get; }
}

/// <summary>
/// Lesson 12: dependency injection and service lifetimes.
/// </summary>
public static class DependencyInjectionExamples
{
    public static ServiceProvider CreateProvider()
    {
        ServiceCollection services = new();

        // Singleton: one instance for the entire service provider lifetime.
        services.AddSingleton<IClock, SystemClock>();

        // Transient: a new instance is created each time it is requested.
        services.AddTransient<IGreetingFormatter, GreetingFormatter>();
        services.AddTransient<TransientMarker>();

        // Scoped: one instance per created scope, normally one web request.
        services.AddScoped<RequestContext>();
        services.AddScoped<GreetingService>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            // These checks catch invalid registrations while the application
            // starts instead of allowing a hidden runtime failure later.
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    public static LifetimeObservation ObserveLifetimes()
    {
        using ServiceProvider provider = CreateProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();

        IClock singletonFromFirstScope = firstScope.ServiceProvider.GetRequiredService<IClock>();
        IClock singletonFromSecondScope = secondScope.ServiceProvider.GetRequiredService<IClock>();

        TransientMarker transientOne = firstScope.ServiceProvider.GetRequiredService<TransientMarker>();
        TransientMarker transientTwo = firstScope.ServiceProvider.GetRequiredService<TransientMarker>();

        RequestContext firstRequestOne = firstScope.ServiceProvider.GetRequiredService<RequestContext>();
        RequestContext firstRequestTwo = firstScope.ServiceProvider.GetRequiredService<RequestContext>();
        RequestContext secondRequest = secondScope.ServiceProvider.GetRequiredService<RequestContext>();

        return new LifetimeObservation(
            singletonIsShared: ReferenceEquals(singletonFromFirstScope, singletonFromSecondScope),
            transientIsNewEachTime: !ReferenceEquals(transientOne, transientTwo),
            scopedIsSharedWithinScope: ReferenceEquals(firstRequestOne, firstRequestTwo),
            scopedIsNewForAnotherScope: !ReferenceEquals(firstRequestOne, secondRequest));
    }
}
