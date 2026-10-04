using System.Text.Json.Serialization;
using AdvancedAspNetCoreLab.Background;
using AdvancedAspNetCoreLab.Caching;
using AdvancedAspNetCoreLab.Configuration;
using AdvancedAspNetCoreLab.DI;
using AdvancedAspNetCoreLab.Models;
using Microsoft.Extensions.Options;

namespace AdvancedAspNetCoreLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        int outputCacheSeconds = builder.Configuration.GetValue(
            "CoursePlatform:OutputCacheSeconds",
            5);

        // Options bind configuration to a typed object and validate it while
        // the host starts, rather than discovering a bad value during a request.
        builder.Services
            .AddOptions<CoursePlatformOptions>()
            .Bind(builder.Configuration.GetSection(CoursePlatformOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => options.QueueName.All(character =>
                    char.IsLetterOrDigit(character) || character == '-'),
                "QueueName must contain only letters, digits, or hyphens.")
            .ValidateOnStart();

        // Keyed services allow multiple implementations behind one interface.
        builder.Services.AddKeyedSingleton<IClock, SystemClock>("system");
        builder.Services.AddKeyedSingleton<IClock, FixedClock>("fixed");
        builder.Services.AddSingleton<IClockFactory, ClockFactory>();

        // Open generic registration lets DI construct IMessageEnvelope<T> for
        // any closed T without registering every possible type separately.
        builder.Services.AddTransient(typeof(IMessageEnvelope<>), typeof(MessageEnvelope<>));

        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSingleton<CatalogCacheService>();

        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("Short", policy =>
                policy.Expire(TimeSpan.FromSeconds(outputCacheSeconds)));
        });
        builder.Services.AddResponseCaching();

        // The queue is bounded so producers experience backpressure instead of
        // allowing unbounded memory growth during a traffic spike.
        builder.Services.AddSingleton<IBackgroundTaskQueue>(
            new BackgroundTaskQueue(capacity: 100));
        builder.Services.AddSingleton<JobStore>();
        builder.Services.AddScoped<CatalogRefreshHandler>();
        builder.Services.AddHostedService<QueuedBackgroundService>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddProblemDetails();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseResponseCaching();
        app.UseOutputCache();

        app.MapGet("/api/config", (
            IOptions<CoursePlatformOptions> options,
            IHostEnvironment environment) =>
        {
            CoursePlatformOptions settings = options.Value;
            return Results.Ok(new
            {
                settings.DisplayName,
                settings.MemoryCacheSeconds,
                settings.OutputCacheSeconds,
                settings.QueueName,
                environment = environment.EnvironmentName
            });
        });

        app.MapGet("/api/di", (
            IClockFactory clockFactory,
            IMessageEnvelope<CatalogItem> envelope,
            IOptions<CoursePlatformOptions> options,
            ILogger<Program> logger) =>
        {
            using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["Feature"] = "AdvancedDI",
                ["QueueName"] = options.Value.QueueName
            });

            IClock systemClock = clockFactory.Create("system");
            IClock fixedClock = clockFactory.Create("fixed");
            CatalogItem sample = new(1, "DI sample", "Advanced");

            logger.LogInformation(
                "Resolved keyed clocks and an open generic envelope for {ItemTitle}",
                sample.Title);

            return Results.Ok(new
            {
                systemClock = systemClock.Source,
                fixedClock = fixedClock.Source,
                envelope = envelope.Describe(sample),
                systemTime = systemClock.UtcNow,
                fixedTime = fixedClock.UtcNow
            });
        });

        app.MapGet("/api/cache/memory", async (
            CatalogCacheService cache,
            CancellationToken cancellationToken) =>
        {
            CatalogSnapshot snapshot = await cache.GetAsync(cancellationToken);
            return Results.Ok(snapshot);
        });

        app.MapGet("/api/cache/output", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Generating a response that can be output cached");
            return Results.Ok(new { generatedAtUtc = DateTimeOffset.UtcNow });
        }).CacheOutput("Short");

        app.MapGet("/api/cache/response", (HttpContext context) =>
        {
            // Response caching follows HTTP cache headers and lets clients or
            // proxies decide whether this public response can be reused.
            context.Response.Headers.CacheControl = "public,max-age=5";
            return Results.Ok(new { generatedAtUtc = DateTimeOffset.UtcNow });
        });

        app.MapPost("/api/jobs", async (
            IBackgroundTaskQueue queue,
            JobStore jobs,
            IServiceScopeFactory scopeFactory,
            IOptions<CoursePlatformOptions> options,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            Guid jobId = jobs.Create();

            await queue.QueueAsync(async stoppingToken =>
            {
                // Hosted services are singletons. Create a scope before
                // resolving the scoped job handler used by this work item.
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                CatalogRefreshHandler handler = scope.ServiceProvider
                    .GetRequiredService<CatalogRefreshHandler>();
                await handler.ProcessAsync(jobId, stoppingToken);
            }, cancellationToken);

            logger.LogInformation(
                "Queued background job {JobId} on {QueueName}",
                jobId,
                options.Value.QueueName);

            return Results.Accepted(
                $"/api/jobs/{jobId}",
                new { id = jobId, state = JobState.Pending, queue = options.Value.QueueName });
        });

        app.MapGet("/api/jobs/{id:guid}", (Guid id, JobStore jobs) =>
        {
            return jobs.TryGet(id, out JobStatus? status)
                ? Results.Ok(status)
                : Results.NotFound();
        });

        app.Run();
    }
}
