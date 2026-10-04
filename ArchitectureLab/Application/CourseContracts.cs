using ArchitectureLab.Domain;

namespace ArchitectureLab.Application;

public sealed record CourseResponse(
    Guid Id,
    string Title,
    string Level,
    decimal Price);

public sealed record CreateCourseRequest(
    string Title,
    string Level,
    decimal Price);

public sealed record CreateCourseCommand(
    Guid Id,
    string Title,
    string Level,
    decimal Price) : IRequest<CourseResponse>;

public sealed record GetCourseQuery(Guid Id) : IRequest<CourseResponse?>;

public sealed record ListCoursesQuery(string? Level) : IRequest<IReadOnlyList<CourseResponse>>;

public static class CourseResponseMapper
{
    public static CourseResponse Map(Course course)
    {
        return new(
            course.Id.Value,
            course.Title,
            course.Level,
            course.Price);
    }
}
