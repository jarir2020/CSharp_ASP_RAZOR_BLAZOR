using ArchitectureLab.Application;
using ArchitectureLab.Domain;
using ArchitectureLab.Domain.Specifications;
using ArchitectureLab.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CSharpCore.Tests;

public sealed class ArchitectureLabTests
{
    [Fact]
    public void Course_aggregate_canonicalizes_level_and_rounds_price()
    {
        Course course = Course.Create(
            new CourseId(Guid.NewGuid()),
            " Architecture Patterns ",
            "beginner",
            99.999m);

        Assert.Equal("Architecture Patterns", course.Title);
        Assert.Equal("Beginner", course.Level);
        Assert.Equal(100.00m, course.Price);
    }

    [Fact]
    public void Course_aggregate_rejects_an_unknown_level()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Course.Create(
                new CourseId(Guid.NewGuid()),
                "Architecture Patterns",
                "Expert",
                100m));

        Assert.Contains("Course level", exception.Message);
    }

    [Fact]
    public void Specification_matches_only_the_requested_level()
    {
        Course beginner = Course.Create(
            new CourseId(Guid.NewGuid()),
            "C# Basics",
            "Beginner",
            50m);
        Course advanced = Course.Create(
            new CourseId(Guid.NewGuid()),
            "Distributed Systems",
            "Advanced",
            200m);
        CourseLevelSpecification specification = new("advanced");

        Assert.False(specification.IsSatisfiedBy(beginner));
        Assert.True(specification.IsSatisfiedBy(advanced));
    }

    [Fact]
    public async Task Dispatcher_routes_a_command_to_a_handler_and_query_reads_the_result()
    {
        ServiceCollection services = new();
        services.AddArchitectureApplication();
        await using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IRequestDispatcher dispatcher = scope.ServiceProvider
            .GetRequiredService<IRequestDispatcher>();
        Guid id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        CourseResponse created = await dispatcher.SendAsync(
            new CreateCourseCommand(id, "Testing Architecture", "Advanced", 175m));
        CourseResponse? loaded = await dispatcher.SendAsync(new GetCourseQuery(id));

        Assert.Equal(id, created.Id);
        Assert.Equal("Testing Architecture", loaded?.Title);
        Assert.Equal(175m, loaded?.Price);
    }

    [Fact]
    public async Task List_query_uses_a_specification_to_filter_the_repository()
    {
        ServiceCollection services = new();
        services.AddArchitectureApplication();
        await using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        IRequestDispatcher dispatcher = scope.ServiceProvider
            .GetRequiredService<IRequestDispatcher>();

        IReadOnlyList<CourseResponse> courses = await dispatcher.SendAsync(
            new ListCoursesQuery("intermediate"));

        Assert.Single(courses);
        Assert.Equal("ASP.NET Core APIs", courses[0].Title);
    }
}
