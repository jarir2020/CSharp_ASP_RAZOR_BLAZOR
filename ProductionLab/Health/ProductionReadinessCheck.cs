using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ProductionLab.Configuration;

namespace ProductionLab.Health;

public sealed class ProductionReadinessCheck(
    IOptions<ProductionOptions> options,
    IHostEnvironment environment) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        ProductionOptions settings = options.Value;

        // Development and test runs can use a local placeholder. Production
        // requires the secret to be injected before the host becomes ready.
        if (environment.IsProduction() && !settings.SecretConfigured)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "The required external API secret has not been configured."));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Configuration is ready for {settings.ServiceName}."));
    }
}
