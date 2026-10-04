using ArchitectureLab.Application;

namespace ArchitectureLab.Api.Features.Courses;

public static class ListCoursesSlice
{
    public static RouteGroupBuilder MapListCoursesSlice(
        this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            string? level,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            IReadOnlyList<CourseResponse> response = await dispatcher.SendAsync(
                new ListCoursesQuery(level),
                cancellationToken);

            return Results.Ok(response);
        });

        return group;
    }
}
