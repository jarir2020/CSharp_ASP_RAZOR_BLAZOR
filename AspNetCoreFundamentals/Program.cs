using AspNetCoreFundamentals.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// WebApplication.CreateBuilder loads appsettings.json, environment-specific
// appsettings files, environment variables, and command-line configuration.
// The service collection is where application dependencies are registered.
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

// Middleware runs in the order it is added. Exception handling belongs near
// the beginning so it can observe failures from later pipeline components.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title = "An unexpected error occurred.",
            status = StatusCodes.Status500InternalServerError,
            requestId = context.Items["RequestId"]
        });
    });
});

app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Explicit UseRouting makes the routing stage visible while learning the
// pipeline. Endpoint mapping below connects routes to request handlers.
app.UseRouting();

app.MapGet("/", (IConfiguration configuration, IHostEnvironment environment) =>
{
    return Results.Ok(new
    {
        application = configuration["Course:ApplicationName"] ?? "Unknown application",
        message = configuration["Course:WelcomeMessage"] ?? "Hello from ASP.NET Core.",
        environment = environment.EnvironmentName,
        framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
    });
});

RouteGroupBuilder api = app.MapGroup("/api");

// The optional route parameter allows both /api/hello and /api/hello/Jarir.
api.MapGet("/hello/{name?}", (
    string? name,
    HttpContext context,
    ILogger<Program> logger) =>
{
    string displayName = string.IsNullOrWhiteSpace(name) ? "Guest" : name.Trim();
    string requestId = context.Items["RequestId"]?.ToString() ?? "missing";

    logger.LogInformation("Creating a greeting for {Name}", displayName);

    return Results.Ok(new
    {
        greeting = $"Hello, {displayName}!",
        requestId
    });
});

// The :int constraint prevents text such as /api/items/not-a-number from
// reaching this handler. Routing returns a 404 when the constraint fails.
api.MapGet("/items/{id:int}", GetItem);

api.MapGet("/request-info", (HttpContext context) =>
{
    return Results.Ok(new
    {
        method = context.Request.Method,
        path = context.Request.Path.ToString(),
        requestId = context.Items["RequestId"]?.ToString() ?? "missing",
        accepts = context.Request.Headers["Accept"].ToString()
    });
});

app.MapGet("/error-demo", () =>
{
    throw new InvalidOperationException("This endpoint demonstrates exception middleware.");
});

app.Run();

static IResult GetItem(int id)
{
    if (id == 1)
    {
        return Results.Ok(new { id, name = "C# Fundamentals" });
    }

    return Results.NotFound(new
    {
        error = "Item not found",
        id
    });
}

// WebApplicationFactory uses this partial class to locate the application in
// integration tests. It is generated from the top-level statements above.
public partial class Program
{
}
