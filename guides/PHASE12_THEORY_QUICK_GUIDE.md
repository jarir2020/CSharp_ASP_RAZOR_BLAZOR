# Phase 12: Advanced ASP.NET Core

Phase 12 introduces `AdvancedAspNetCoreLab`, a small minimal API that puts
several production infrastructure patterns next to each other. The endpoint
surface is deliberately small so the lifetime, configuration, cache, logging,
and background-work decisions remain visible.

## 1. Typed configuration and the options pattern

`appsettings.json` contains a `CoursePlatform` section. `Program.cs` binds it
to `CoursePlatformOptions`:

```csharp
builder.Services
    .AddOptions<CoursePlatformOptions>()
    .Bind(builder.Configuration.GetSection(CoursePlatformOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

The options class contains validation attributes for required strings and
allowed numeric ranges. The custom validator checks that the queue name uses
letters, digits, or hyphens as well.
`ValidateOnStart` makes a bad configuration fail during host startup instead
of waiting for the first request that needs the option.

Configuration providers are layered. Environment variables can override the
same setting with a double underscore, for example:

```text
CoursePlatform__OutputCacheSeconds=20
```

Keep secrets out of `appsettings.json`; use environment variables or a secret
manager for credentials and keys.

## 2. Advanced dependency injection

The `/api/di` endpoint demonstrates three DI patterns.

### Keyed services

Two `IClock` implementations share one interface:

```csharp
builder.Services.AddKeyedSingleton<IClock, SystemClock>("system");
builder.Services.AddKeyedSingleton<IClock, FixedClock>("fixed");
```

`ClockFactory` resolves the requested key through
`GetRequiredKeyedService<IClock>`. The factory gives the rest of the
application a single, named creation boundary.

### Open generics

The registration below supports every closed `IMessageEnvelope<T>` request:

```csharp
builder.Services.AddTransient(
    typeof(IMessageEnvelope<>),
    typeof(MessageEnvelope<>));
```

The endpoint asks for `IMessageEnvelope<CatalogItem>` and DI constructs the
matching closed implementation automatically.

### Service scopes

The background queue is hosted as a singleton, while
`CatalogRefreshHandler` is scoped. The queued delegate creates an async scope
before resolving the handler. A singleton hosted service must not capture a
scoped service directly because the scoped object would outlive its intended
scope.

## 3. Structured logging and scopes

The lab uses `ILogger<T>` with message templates:

```csharp
logger.LogInformation(
    "Queued background job {JobId} on {QueueName}",
    jobId,
    options.Value.QueueName);
```

The DI and cache examples also use `BeginScope` to attach context such as the
feature name and cache key to every log entry created inside the scope. Keep
values structured so a centralized logging system can search and aggregate
them. Do not log passwords, access tokens, or sensitive personal data.

The built-in logging abstractions keep application code independent from a
particular provider. Serilog or another provider can be added later when the
deployment needs centralized sinks, enrichment, or durable log storage.

## 4. Memory and distributed caching

`CatalogCacheService` checks two layers:

```text
IMemoryCache
  -> IDistributedCache
      -> origin data
```

The sample registers `AddDistributedMemoryCache`, which implements the
distributed-cache abstraction in the current process. It teaches the API shape
without requiring Redis. A multi-instance deployment should replace it with a
shared provider such as Redis when cache consistency across servers matters.

The first request generates the catalog. A later request can be served from
the memory layer. If the memory entry expires but the distributed entry still
exists, the service promotes the distributed value back into memory.

Cache entries need an expiration and a clear invalidation strategy. Do not put
user-specific or authorization-sensitive data in a shared cache without
varying the key and reviewing the security boundary.

## 5. Output caching

The `/api/cache/output` endpoint uses a named output-cache policy:

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Short", policy => policy.Expire(TimeSpan.FromSeconds(5)));
});

app.MapGet("/api/cache/output", Handler).CacheOutput("Short");
```

Output caching is server-controlled response reuse. The cache key includes the
request details that distinguish responses, including the query string by
default. The pipeline places `UseOutputCache` after routing, and authentication
and authorization must run before it when protected endpoints are cached.

The short policy makes the generated timestamp easy to observe. Output cache
storage is local to the process in this sample; a multi-server deployment needs
a shared output-cache store when responses must be shared.

## 6. Response caching

The `/api/cache/response` endpoint sets:

```text
Cache-Control: public,max-age=5
```

Response caching follows HTTP cache semantics and lets clients or intermediary
proxies decide whether the response can be reused. Output caching is usually a
better fit when the server must control reuse independently of request cache
headers. Treat response headers as part of the public data contract.

## 7. Queued background work

`BackgroundTaskQueue` uses a bounded `Channel<Func<CancellationToken, ValueTask>>`.
The `/api/jobs` endpoint adds work and returns `202 Accepted` with a job ID.
`QueuedBackgroundService` reads work sequentially until application shutdown.

```text
POST /api/jobs
  -> Pending
  -> bounded channel
  -> BackgroundService
  -> scoped CatalogRefreshHandler
  -> Running
  -> Completed or Failed
```

The bounded channel applies backpressure. A production queue may need durable
delivery, retries, idempotency, dead-letter handling, metrics, and a separate
worker process. In-memory work is lost when the application stops.

The hosted service honors the application cancellation token. Long-running
work should pass that token to its I/O calls and stop promptly during graceful
shutdown.

## 8. Testing infrastructure code

`AdvancedAspNetCoreTests` exercises configuration and DI resolution, memory
cache reuse, output and response cache behavior, and the queued job lifecycle.
The test factory uses the Testing environment so the app does not attempt an
HTTPS redirect inside the in-memory test host.

## Run the phase

From the repository root:

```bash
dotnet run --project AdvancedAspNetCoreLab/AdvancedAspNetCoreLab.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

Example requests:

```bash
curl http://localhost:5488/api/config
curl http://localhost:5488/api/di
curl http://localhost:5488/api/cache/memory
curl http://localhost:5488/api/cache/output
curl -X POST http://localhost:5488/api/jobs
```

The next phase covers broader web-development features such as OpenAPI,
health checks, uploads, streaming, pagination, SignalR, WebSockets, and gRPC.
