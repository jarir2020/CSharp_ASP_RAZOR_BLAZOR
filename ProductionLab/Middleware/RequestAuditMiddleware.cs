using System.Diagnostics;
using ProductionLab.Models;
using ProductionLab.Services;

namespace ProductionLab.Middleware;

public sealed class RequestAuditMiddleware(
    RequestDelegate next,
    AuditStore auditStore,
    ILogger<RequestAuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        string correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? "missing";
        AuditEvent auditEvent = new(
            DateTimeOffset.UtcNow,
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);

        auditStore.Add(auditEvent);
        logger.LogInformation(
            "HTTP {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms",
            auditEvent.Method,
            auditEvent.Path,
            auditEvent.StatusCode,
            auditEvent.ElapsedMilliseconds);
    }
}
