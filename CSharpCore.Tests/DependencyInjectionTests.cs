using CSharpCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CSharpCore.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Constructor_injection_allows_deterministic_dependencies_in_tests()
    {
        FixedClock clock = new(new DateTimeOffset(2026, 10, 5, 12, 30, 0, TimeSpan.Zero));
        GreetingService service = new(clock, new GreetingFormatter());

        Assert.Equal("Hello, Jarir! UTC: 2026-10-05 12:30:00", service.CreateGreeting(" Jarir "));
    }

    [Fact]
    public void Container_resolves_a_service_and_its_constructor_dependencies()
    {
        using ServiceProvider provider = DependencyInjectionExamples.CreateProvider();
        using IServiceScope scope = provider.CreateScope();

        GreetingService service = scope.ServiceProvider.GetRequiredService<GreetingService>();

        Assert.StartsWith("Hello, Jarir! UTC: ", service.CreateGreeting(" Jarir "));
    }

    [Fact]
    public void Registered_lifetimes_have_different_reuse_rules()
    {
        LifetimeObservation observation = DependencyInjectionExamples.ObserveLifetimes();

        Assert.True(observation.SingletonIsShared);
        Assert.True(observation.TransientIsNewEachTime);
        Assert.True(observation.ScopedIsSharedWithinScope);
        Assert.True(observation.ScopedIsNewForAnotherScope);
    }
}
