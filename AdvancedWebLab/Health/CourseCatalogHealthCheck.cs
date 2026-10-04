using AdvancedWebLab.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdvancedWebLab.Health;

public sealed class CourseCatalogHealthCheck(CourseCatalog catalog) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(catalog.Count > 0
            ? HealthCheckResult.Healthy($"The catalog contains {catalog.Count} courses.")
            : HealthCheckResult.Unhealthy("The catalog is empty."));
    }
}
