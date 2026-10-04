using ArchitectureLab.Domain;
using ArchitectureLab.Domain.Specifications;

namespace ArchitectureLab.Application;

public sealed class CreateCourseHandler(
    ICourseRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        Course course = Course.Create(
            new CourseId(request.Id),
            request.Title,
            request.Level,
            request.Price);

        await repository.AddAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return CourseResponseMapper.Map(course);
    }
}

public sealed class GetCourseHandler(
    ICourseRepository repository) : IRequestHandler<GetCourseQuery, CourseResponse?>
{
    public async Task<CourseResponse?> HandleAsync(
        GetCourseQuery request,
        CancellationToken cancellationToken)
    {
        Course? course = await repository.GetByIdAsync(
            new CourseId(request.Id),
            cancellationToken);

        return course is null ? null : CourseResponseMapper.Map(course);
    }
}

public sealed class ListCoursesHandler(
    ICourseRepository repository) : IRequestHandler<ListCoursesQuery, IReadOnlyList<CourseResponse>>
{
    public async Task<IReadOnlyList<CourseResponse>> HandleAsync(
        ListCoursesQuery request,
        CancellationToken cancellationToken)
    {
        CourseLevelSpecification specification = new(request.Level);
        IReadOnlyList<Course> courses = await repository.ListAsync(
            specification,
            cancellationToken);

        return courses
            .Select(CourseResponseMapper.Map)
            .ToList();
    }
}
