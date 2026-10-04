using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MvcLab.Infrastructure.Filters;

public sealed class RequestAuditFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        ActionExecutedContext executed = await next();
        stopwatch.Stop();

        // An action filter can add cross-cutting behavior without putting the
        // same timing or audit code in every controller action.
        if (executed.Exception is null && !context.HttpContext.Response.HasStarted)
        {
            context.HttpContext.Response.Headers["X-Mvc-Filter"] = nameof(RequestAuditFilter);
            context.HttpContext.Response.Headers["X-Mvc-Elapsed-Milliseconds"] =
                stopwatch.ElapsedMilliseconds.ToString();
        }
    }
}
