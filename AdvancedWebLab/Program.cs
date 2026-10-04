using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AdvancedWebLab.Health;
using AdvancedWebLab.Hubs;
using AdvancedWebLab.Models;
using AdvancedWebLab.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;

namespace AdvancedWebLab;

public partial class Program
{
    private const long MaximumUploadBytes = 1_048_576;

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<CourseCatalog>();
        builder.Services
            .AddHealthChecks()
            .AddCheck<CourseCatalogHealthCheck>(
                "course-catalog",
                tags: ["ready"]);

        // SignalR uses a hub protocol and selects an available transport for
        // each client, usually WebSockets when the environment supports it.
        builder.Services.AddSignalR();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // This deliberately small policy makes the rate-limit behavior
            // observable from curl and from the integration test.
            options.AddFixedWindowLimiter("course-demo", limiterOptions =>
            {
                limiterOptions.PermitLimit = 2;
                limiterOptions.Window = TimeSpan.FromSeconds(30);
                limiterOptions.QueueLimit = 0;
                limiterOptions.AutoReplenishment = true;
            });
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        WebApplication app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        // Enable the HTTP upgrade path before the WebSocket endpoint runs.
        app.UseWebSockets();
        app.UseRouting();
        app.UseRateLimiter();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // This small contract endpoint keeps the OpenAPI shape visible without
        // hiding the lesson behind generated files or an external UI package.
        app.MapGet("/openapi/v1.json", () => Results.Json(BuildOpenApiDocument()));
        app.MapGet("/swagger", () => Results.Content(
            """
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><title>Advanced Web Lab API</title></head>
            <body>
              <h1>Advanced Web Lab API</h1>
              <p>This lesson exposes its OpenAPI contract at <a href="/openapi/v1.json">/openapi/v1.json</a>.</p>
            </body>
            </html>
            """,
            "text/html"));

        app.MapGet("/api/v1/courses", (
            int? page,
            int? pageSize,
            string? search,
            string? sort,
            CourseCatalog catalog,
            HttpContext context) =>
        {
            int currentPage = Math.Max(page ?? 1, 1);
            int currentPageSize = Math.Clamp(pageSize ?? 3, 1, 20);
            IReadOnlyList<Course> allMatches = catalog.Search(search, sort);
            IReadOnlyList<Course> items = allMatches
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            context.Response.Headers["X-Api-Version"] = "1.0";
            return Results.Ok(new PagedResult<Course>(
                currentPage,
                currentPageSize,
                allMatches.Count,
                items));
        });

        app.MapGet("/api/v2/courses", (
            int? page,
            int? pageSize,
            string? search,
            string? sort,
            CourseCatalog catalog,
            HttpContext context) =>
        {
            int currentPage = Math.Max(page ?? 1, 1);
            int currentPageSize = Math.Clamp(pageSize ?? 3, 1, 20);
            IReadOnlyList<Course> allMatches = catalog.Search(search, sort);
            var items = allMatches
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .Select(course => new
                {
                    course.Id,
                    course.Title,
                    course.Level,
                    course.Summary,
                    durationMinutes = course.Id * 30
                })
                .ToList();

            context.Response.Headers["X-Api-Version"] = "2.0";
            return Results.Ok(new
            {
                page = currentPage,
                pageSize = currentPageSize,
                totalItems = allMatches.Count,
                items
            });
        });

        app.MapGet("/api/v1/limited", () => Results.Ok(new
        {
            message = "This endpoint uses a two-request fixed-window policy."
        })).RequireRateLimiting("course-demo");

        app.MapPost("/api/v1/files", async (
            HttpRequest request,
            CancellationToken cancellationToken) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Send multipart/form-data with a file field." });
            }

            IFormCollection form = await request.ReadFormAsync(cancellationToken);
            IFormFile? file = form.Files.GetFile("file");

            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "A non-empty file field is required." });
            }

            if (file.Length > MaximumUploadBytes)
            {
                return Results.BadRequest(new { error = "The file is larger than 1 MiB." });
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] allowedExtensions = [".csv", ".json", ".txt"];
            if (!allowedExtensions.Contains(extension))
            {
                return Results.BadRequest(new { error = "Only .csv, .json, and .txt files are accepted." });
            }

            // The sample consumes the stream but deliberately does not persist
            // an uploaded file. A real app would scan, authorize, and store it.
            await using Stream stream = file.OpenReadStream();
            await stream.CopyToAsync(Stream.Null, cancellationToken);

            return Results.Ok(new
            {
                fileName = Path.GetFileName(file.FileName),
                file.Length,
                file.ContentType,
                extension
            });
        });

        app.MapGet("/api/v1/stream", async (HttpResponse response, CancellationToken cancellationToken) =>
        {
            response.ContentType = "application/x-ndjson";

            for (int index = 1; index <= 4; index++)
            {
                string line = JsonSerializer.Serialize(new
                {
                    sequence = index,
                    message = $"stream item {index}"
                }) + "\n";

                await response.WriteAsync(line, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
                await Task.Delay(5, cancellationToken);
            }
        });

        app.Map("/ws/echo", (RequestDelegate)(async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    new { error = "Upgrade this request to a WebSocket connection." });
                return;
            }

            using WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            byte[] buffer = new byte[4_096];
            WebSocketReceiveResult received = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                context.RequestAborted);

            if (received.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Echo connection closed",
                    context.RequestAborted);
                return;
            }

            await socket.SendAsync(
                buffer.AsMemory(0, received.Count),
                received.MessageType,
                received.EndOfMessage,
                context.RequestAborted);
        }));

        app.MapHub<CourseHub>("/hubs/courses");

        app.Run();
    }

    private static object BuildOpenApiDocument()
    {
        return new
        {
            openapi = "3.0.3",
            info = new
            {
                title = "Advanced Web Lab API",
                version = "1.0.0",
                description = "A small educational contract for the Phase 13 endpoints."
            },
            paths = new Dictionary<string, object>
            {
                ["/api/v1/courses"] = new
                {
                    get = new
                    {
                        summary = "List courses with pagination, filtering, and sorting",
                        parameters = new[] { "page", "pageSize", "search", "sort" }
                    }
                },
                ["/api/v1/files"] = new
                {
                    post = new
                    {
                        summary = "Upload one small text-like file"
                    }
                },
                ["/health"] = new
                {
                    get = new
                    {
                        summary = "Return application health"
                    }
                }
            }
        };
    }
}
