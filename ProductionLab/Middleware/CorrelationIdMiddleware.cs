namespace ProductionLab.Middleware;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = GetOrCreateCorrelationId(context);
        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId
        });

        await next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        string supplied = context.Request.Headers[HeaderName].ToString().Trim();

        // Limit accepted header size before placing it in logs and response
        // headers. Invalid or oversized values receive a new server ID.
        return supplied.Length is > 0 and <= 80
            ? supplied
            : Guid.NewGuid().ToString("N");
    }
}
