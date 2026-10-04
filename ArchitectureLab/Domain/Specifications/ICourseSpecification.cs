namespace ArchitectureLab.Domain.Specifications;

public interface ICourseSpecification
{
    bool IsSatisfiedBy(Course course);
}

public sealed class CourseLevelSpecification(string? requestedLevel)
    : ICourseSpecification
{
    private readonly string? _requestedLevel = requestedLevel?.Trim();

    public bool IsSatisfiedBy(Course course)
    {
        return string.IsNullOrWhiteSpace(_requestedLevel)
            || string.Equals(course.Level, _requestedLevel, StringComparison.OrdinalIgnoreCase);
    }
}
