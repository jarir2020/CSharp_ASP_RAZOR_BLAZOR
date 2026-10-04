using ArchitectureLab.Application;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureLab.Infrastructure;

public static class ArchitectureServiceCollectionExtensions
{
    public static IServiceCollection AddArchitectureApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();

        // Explicit registrations make the request-to-handler relationship
        // visible while learning. A mediator package can scan these later.
        services.AddScoped<
            IRequestHandler<CreateCourseCommand, CourseResponse>,
            CreateCourseHandler>();
        services.AddScoped<
            IRequestHandler<GetCourseQuery, CourseResponse?>,
            GetCourseHandler>();
        services.AddScoped<
            IRequestHandler<ListCoursesQuery, IReadOnlyList<CourseResponse>>,
            ListCoursesHandler>();

        return services;
    }
}
