using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreWebApi.Infrastructure;
using AspNetCoreWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWebApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddSingleton<IProductCatalog, InMemoryProductCatalog>();

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                // JSON uses camelCase for JavaScript clients while C# properties stay
                // PascalCase in source code. Enum strings are easier to read than 0/1.
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            // [ApiController] automatically checks model state. This factory makes the
            // validation response shape explicit and consistent for this course API.
            options.InvalidModelStateResponseFactory = context =>
            {
                ValidationProblemDetails problem = new(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Request validation failed.",
                    Type = "https://httpstatuses.com/400",
                    Instance = context.HttpContext.Request.Path
                };

                return new BadRequestObjectResult(problem);
            };
        });

        WebApplication app = builder.Build();

        // The exception boundary is before routing and controllers so every API
        // action receives the same handling for known and unknown failures.
        app.UseMiddleware<ApiExceptionMiddleware>();
        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}
