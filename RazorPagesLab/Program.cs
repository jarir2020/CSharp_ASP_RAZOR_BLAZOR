using RazorPagesLab.Services;

namespace RazorPagesLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Razor Pages use PageModel classes for request logic and .cshtml files
        // for the HTML template. Registering the framework service enables both.
        builder.Services.AddRazorPages();

        // A singleton keeps this small lesson's catalog available across HTTP
        // requests. A real application would normally use a database service.
        builder.Services.AddSingleton<WorkshopCatalog>();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // The integration test host uses HTTP only. Normal local and deployed
        // hosts still redirect HTTP requests to their configured HTTPS port.
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();

        app.Run();
    }
}
