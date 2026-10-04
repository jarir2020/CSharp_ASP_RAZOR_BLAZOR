using AspNetCoreWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWebApi.Infrastructure;

/// <summary>
/// A single exception boundary keeps controller actions focused on normal
/// behavior and turns known domain failures into consistent ProblemDetails.
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ProductConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", exception.Message);
        }
        catch (ProductDomainException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity, "Business rule failed", exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Resource not found", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled API exception for {Path}", context.Request.Path);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                detail: null);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string? detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problem = new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
