# Phase 4: ASP.NET Core Fundamentals

Phase 4 begins web development. The `AspNetCoreFundamentals` project is a
small HTTP application that shows how `Program.cs`, configuration, logging,
middleware, routing, and endpoints work together.

## 1. HTTP request and response

An HTTP request contains a method, URL path, headers, and sometimes a body. An
ASP.NET Core response contains a status code, headers, and a body.

```text
GET /api/hello/Jarir HTTP/1.1
Accept: application/json

HTTP/1.1 200 OK
Content-Type: application/json
X-Request-Id: ...
```

The application uses JSON responses because JSON is the common format for web
APIs. Status codes communicate the result:

- `200 OK` means the request succeeded.
- `404 Not Found` means the route or requested item does not exist.
- `500 Internal Server Error` means an unexpected failure reached the global
  exception handler.

Headers carry metadata such as content type, caching instructions, and the
request identifier added by the custom middleware.

## 2. `Program.cs` and application startup

Modern ASP.NET Core applications commonly start with:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
app.Run();
```

The builder prepares configuration, logging, dependency injection, and the web
server. `builder.Services` registers application services. `builder.Build()`
creates the application. Middleware and endpoints are added before `Run()`
starts the server.

`WebApplication.CreateBuilder` loads configuration from sources such as:

1. `appsettings.json`.
2. An environment-specific file such as
   `appsettings.Development.json`.
3. Environment variables.
4. Command-line arguments.

Later providers can override earlier values. Secrets should come from a secret
store or environment configuration rather than committed JSON files.

## 3. Middleware and the request pipeline

Middleware is a chain of components. Each component can inspect or change the
request, call the next component, and inspect or change the response.

```text
Request
  -> Exception handling
  -> Request ID
  -> Request logging
  -> Routing
  -> Endpoint
  -> Response
```

The order matters. Exception handling is near the beginning so it can observe
failures from later components. The request ID runs before logging so the log
entry and response can share the same identifier.

`app.Use(...)` adds middleware that can call the next component. A custom class
with `InvokeAsync(HttpContext)` can be added with `UseMiddleware<T>()`.

`app.Map(...)` branches or maps requests to an endpoint. The Phase 4 project
uses `MapGet` for small route handlers and `MapGroup("/api")` to share a route
prefix.

## 4. Configuration and environments

Configuration is accessed through `IConfiguration`:

```csharp
string? name = configuration["Course:ApplicationName"];
```

The environment is available through `IHostEnvironment`. Common environment
names include `Development`, `Testing`, and `Production`.

Environment names can control diagnostics and behavior, but application logic
should remain explicit. Do not use `Development` as a substitute for an
authorization check, and do not expose secrets or stack traces in production.

## 5. Logging

ASP.NET Core provides `ILogger<T>` through dependency injection:

```csharp
logger.LogInformation("Creating a greeting for {Name}", displayName);
```

The `{Name}` placeholder is a structured field. Structured logs are easier to
search and aggregate than manually concatenated strings. Log useful context,
but avoid passwords, tokens, and other sensitive values.

The request logging middleware records method, path, status code, elapsed time,
and request ID. A production application would send these logs to a central
logging system and add correlation with traces and metrics.

## 6. Routing and endpoints

Routing maps an HTTP method and path to an endpoint. The project demonstrates:

```csharp
app.MapGet("/api/hello/{name?}", handler);
app.MapGet("/api/items/{id:int}", handler);
```

`{name?}` is an optional route parameter, so both `/api/hello` and
`/api/hello/Jarir` match. `{id:int}` applies an integer constraint. A request
such as `/api/items/not-an-integer` fails route matching and returns `404`
without entering the handler.

ASP.NET Core also supports attribute routing on controllers and conventional
routing. Minimal API routing is useful for learning and small endpoints;
controllers will be introduced with the Web API phase.

## 7. Dependency injection in ASP.NET Core

The framework creates many handler dependencies automatically. The Phase 4
handlers receive `IConfiguration`, `IHostEnvironment`, `HttpContext`, and
`ILogger<Program>` as parameters. These are resolved from the application
service provider.

This is the same DI system introduced in Phase 3. ASP.NET Core adds framework
services and uses scopes around HTTP requests.

## 8. Exception handling

The application maps unexpected exceptions to a generic problem response:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "requestId": "..."
}
```

Clients receive a stable public response while the server logs the underlying
failure. Production applications should avoid returning exception messages or
stack traces to untrusted clients.

## Run the application

From the repository root:

```bash
dotnet run --project AspNetCoreFundamentals/AspNetCoreFundamentals.csproj
```

Then try these URLs:

```text
http://localhost:5000/
http://localhost:5000/api/hello
http://localhost:5000/api/hello/Jarir
http://localhost:5000/api/items/1
http://localhost:5000/api/items/not-an-integer
http://localhost:5000/error-demo
```

Run the integration tests with:

```bash
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The next phase builds a proper ASP.NET Core Web API with controllers, model
binding, DTOs, validation, JSON options, and API error handling.
