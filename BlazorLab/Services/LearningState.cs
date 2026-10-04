using BlazorLab.Models;

namespace BlazorLab.Services;

public sealed class LearningState
{
    private readonly List<CourseSummary> _courses = new()
    {
        new(
            1,
            "ASP.NET Core Fundamentals",
            "Understand hosting, middleware, routing, and dependency injection.",
            "Beginner"),
        new(
            2,
            "C# for Backend Developers",
            "Practice modern C# collections, LINQ, async code, and testing.",
            "Intermediate"),
        new(
            3,
            "Blazor Component Design",
            "Build interactive components with state, events, forms, and routing.",
            "Intermediate")
    };

    public event Action? Changed;

    public int ClickCount { get; private set; }

    public CourseSummary? SelectedCourse { get; private set; }

    public string? LastEnrollmentMessage { get; private set; }

    public IReadOnlyList<CourseSummary> Courses => _courses;

    public void IncrementClickCount()
    {
        ClickCount++;
        Changed?.Invoke();
    }

    public void SelectCourse(CourseSummary course)
    {
        SelectedCourse = course;
        Changed?.Invoke();
    }

    public void RecordEnrollment(EnrollmentInput input)
    {
        CourseSummary? course = _courses.FirstOrDefault(item => item.Id == input.CourseId);
        LastEnrollmentMessage = course is null
            ? "The selected course could not be found."
            : $"Thanks, {input.FullName}. Your enrollment request for {course.Title} was recorded.";
        Changed?.Invoke();
    }
}
