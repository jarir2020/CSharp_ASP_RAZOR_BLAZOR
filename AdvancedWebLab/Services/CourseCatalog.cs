using AdvancedWebLab.Models;

namespace AdvancedWebLab.Services;

public sealed class CourseCatalog
{
    private static readonly IReadOnlyList<Course> Courses =
    [
        new(1, "C# Fundamentals", "Beginner", "Types, control flow, classes, and interfaces."),
        new(2, "Modern C#", "Intermediate", "LINQ, async code, nullable reference types, and events."),
        new(3, "ASP.NET Core APIs", "Intermediate", "HTTP endpoints, middleware, validation, and testing."),
        new(4, "Entity Framework Core", "Intermediate", "DbContext, relationships, queries, and transactions."),
        new(5, "Real-Time ASP.NET Core", "Advanced", "SignalR, WebSockets, streaming, and connection design."),
        new(6, "Production Web Systems", "Advanced", "Health checks, rate limits, observability, and deployment.")
    ];

    public int Count => Courses.Count;

    public IReadOnlyList<Course> Search(string? search, string? sort)
    {
        IEnumerable<Course> query = Courses;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(course =>
                course.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                course.Level.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Whitelist sort fields instead of passing a client-provided value to
        // a dynamic query expression or database fragment.
        query = sort?.ToLowerInvariant() switch
        {
            "title" => query.OrderBy(course => course.Title),
            "level" => query.OrderBy(course => course.Level).ThenBy(course => course.Title),
            "newest" => query.OrderByDescending(course => course.Id),
            _ => query.OrderBy(course => course.Id)
        };

        return query.ToList();
    }
}
