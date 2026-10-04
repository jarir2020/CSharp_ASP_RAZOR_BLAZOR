using ArchitectureLab.Application;
using ArchitectureLab.Domain;

namespace ArchitectureLab.Api.Features.Courses;

public static class CreateCourseSlice
{
    public static RouteGroupBuilder MapCreateCourseSlice(
        this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateCourseRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                CourseResponse response = await dispatcher.SendAsync(
                    new CreateCourseCommand(
                        Guid.NewGuid(),
                        request.Title,
                        request.Level,
                        request.Price),
                    cancellationToken);

                return Results.Created($"/api/courses/{response.Id}", response);
            }
            catch (DomainException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        return group;
    }
}
