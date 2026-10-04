using MvcLab.Infrastructure.Filters;
using MvcLab.Services;

namespace MvcLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // MVC combines controllers, model binding, validation, and Razor views.
        builder.Services.AddControllersWithViews(options =>
        {
            // Register the filter globally so every MVC action passes through it.
            options.Filters.AddService<RequestAuditFilter>();
        });

        builder.Services.AddScoped<RequestAuditFilter>();

        // The catalog is intentionally in memory for this learning example.
        builder.Services.AddSingleton<BookCatalog>();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseRouting();

        // Conventional routing maps /Books to BooksController.Index by default.
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Books}/{action=Index}/{id?}");

        app.Run();
    }
}
