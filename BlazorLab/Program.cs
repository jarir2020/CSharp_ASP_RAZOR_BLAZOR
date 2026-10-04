using BlazorLab.Components;
using BlazorLab.Services;

namespace BlazorLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Razor components can be rendered statically and then made interactive
        // on the server when the browser connects to the Blazor circuit.
        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        // Scoped state lives for one Blazor Server circuit, which makes it a
        // useful fit for a user's current UI state in this lesson.
        builder.Services.AddScoped<LearningState>();

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
