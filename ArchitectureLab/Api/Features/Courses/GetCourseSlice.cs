using ArchitectureLab.Application;

namespace ArchitectureLab.Api.Features.Courses;

public static class GetCourseSlice
{
    public static RouteGroupBuilder MapGetCourseSlice(
        this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (
            Guid id,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            CourseResponse? response = await dispatcher.SendAsync(
                new GetCourseQuery(id),
                cancellationToken);

            return response is null
                ? Results.NotFound()
                : Results.Ok(response);
        });

        return group;
    }
}
