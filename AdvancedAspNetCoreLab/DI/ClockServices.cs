namespace AdvancedAspNetCoreLab.DI;

public interface IClock
{
    string Source { get; }

    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public string Source => "system-keyed-clock";

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class FixedClock : IClock
{
    public string Source => "fixed-keyed-clock";

    public DateTimeOffset UtcNow => new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}

public interface IClockFactory
{
    IClock Create(string name);
}

public sealed class ClockFactory : IClockFactory
{
    private readonly IServiceProvider _services;

    public ClockFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IClock Create(string name)
    {
        // The factory hides keyed-service lookup from the rest of the app.
        return _services.GetRequiredKeyedService<IClock>(name);
    }
}
