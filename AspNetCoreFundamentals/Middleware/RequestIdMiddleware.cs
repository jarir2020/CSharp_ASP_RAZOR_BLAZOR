namespace AspNetCoreFundamentals.Middleware;

/// <summary>
/// Middleware is a component in the HTTP request pipeline. This component
/// creates a request identifier before the endpoint runs and exposes it in the
/// response headers so a client can correlate a response with a log entry.
/// </summary>
public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string requestId = Guid.NewGuid().ToString("N");

        // Items stores data for the current request only. Later middleware or
        // an endpoint can read this value without using a global variable.
        context.Items["RequestId"] = requestId;
        context.Response.Headers["X-Request-Id"] = requestId;

        await _next(context);
    }
}
