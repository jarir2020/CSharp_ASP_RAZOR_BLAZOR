using ArchitectureLab.Api.Features.Courses;
using ArchitectureLab.Infrastructure;

namespace ArchitectureLab;

public partial class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddArchitectureApplication();

        WebApplication app = builder.Build();

        // Each feature maps its own endpoint and keeps request-specific code
        // close to the route, which is the vertical slice idea.
        RouteGroupBuilder courses = app.MapGroup("/api/courses");
        courses.MapCreateCourseSlice();
        courses.MapGetCourseSlice();
        courses.MapListCoursesSlice();

        app.Run();
    }
}
