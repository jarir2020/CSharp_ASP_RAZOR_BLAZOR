using System.Collections.Concurrent;
using ArchitectureLab.Application;
using ArchitectureLab.Domain;
using ArchitectureLab.Domain.Specifications;

namespace ArchitectureLab.Infrastructure;

public sealed class InMemoryCourseRepository : ICourseRepository
{
    private readonly ConcurrentDictionary<Guid, Course> _courses = new();

    public InMemoryCourseRepository()
    {
        Course fundamentals = Course.Create(
            new CourseId(Guid.Parse("11111111-1111-1111-1111-111111111111")),
            "C# Fundamentals",
            "Beginner",
            100m);
        Course api = Course.Create(
            new CourseId(Guid.Parse("22222222-2222-2222-2222-222222222222")),
            "ASP.NET Core APIs",
            "Intermediate",
            150m);
        Course architecture = Course.Create(
            new CourseId(Guid.Parse("33333333-3333-3333-3333-333333333333")),
            "Distributed Architecture",
            "Advanced",
            225m);

        _courses[fundamentals.Id.Value] = fundamentals;
        _courses[api.Id.Value] = api;
        _courses[architecture.Id.Value] = architecture;
    }

    public Task<Course?> GetByIdAsync(
        CourseId id,
        CancellationToken cancellationToken)
    {
        _courses.TryGetValue(id.Value, out Course? course);
        return Task.FromResult(course);
    }

    public Task<IReadOnlyList<Course>> ListAsync(
        ICourseSpecification specification,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Course> courses = _courses.Values
            .Where(specification.IsSatisfiedBy)
            .OrderBy(course => course.Title)
            .ToList();

        return Task.FromResult(courses);
    }

    public Task AddAsync(
        Course course,
        CancellationToken cancellationToken)
    {
        if (!_courses.TryAdd(course.Id.Value, course))
        {
            throw new InvalidOperationException(
                $"Course {course.Id.Value} already exists.");
        }

        return Task.CompletedTask;
    }
}
