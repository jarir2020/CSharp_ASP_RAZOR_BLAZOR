using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using ProductionLab.Configuration;
using ProductionLab.Health;
using ProductionLab.Middleware;
using ProductionLab.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ProductionLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddOptions<ProductionOptions>()
            .Bind(builder.Configuration.GetSection(ProductionOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                settings => !builder.Environment.IsProduction() || settings.SecretConfigured,
                "Production__ExternalApiKey must be provided in Production.")
            .ValidateOnStart();

        builder.Services.AddSingleton<AuditStore>();
        builder.Services.AddHealthChecks()
            .AddCheck<ProductionReadinessCheck>("configuration", tags: ["ready"]);
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddProblemDetails();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownProxies.Add(IPAddress.Loopback);
        });

        // JSON console logging is easy for a log collector to parse. The
        // correlation middleware adds a searchable ID to each request scope.
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "O";
        });

        WebApplication app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseExceptionHandler();
            app.UseHsts();
            // Trust forwarded scheme and client IP only from the explicitly
            // configured reverse proxy. Adjust KnownProxies for deployment.
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestAuditMiddleware>();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.MapGet("/api/config/safe", (
            IOptions<ProductionOptions> options,
            IHostEnvironment environment) =>
        {
            ProductionOptions settings = options.Value;

            // Never return the secret itself. This endpoint reports only the
            // presence of configuration needed for an operational check.
            return Results.Ok(new
            {
                serviceName = settings.ServiceName,
                shutdownTimeoutSeconds = settings.ShutdownTimeoutSeconds,
                secretConfigured = settings.SecretConfigured,
                environment = environment.EnvironmentName
            });
        });

        app.MapGet("/api/runtime", (IOptions<ProductionOptions> options) =>
        {
            Process process = Process.GetCurrentProcess();
            TimeSpan uptime = DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime();

            return Results.Ok(new
            {
                serviceName = options.Value.ServiceName,
                processId = Environment.ProcessId,
                framework = RuntimeInformation.FrameworkDescription,
                operatingSystem = RuntimeInformation.OSDescription,
                startedAtUtc = process.StartTime.ToUniversalTime(),
                uptimeSeconds = Math.Max(0, (long)uptime.TotalSeconds)
            });
        });

        app.MapGet("/api/audit", (int? count, AuditStore auditStore) =>
            Results.Ok(auditStore.Latest(count ?? 10)));

        app.Run();
    }
}
