namespace ArchitectureLab.Domain;

// CourseId is a value object: callers cannot accidentally pass an unrelated
// Guid where the domain expects a course identity.
public readonly record struct CourseId(Guid Value)
{
    public static CourseId New() => new(Guid.NewGuid());
}

// Course is the aggregate root for the small course domain. Its factory keeps
// invariants in one place instead of spreading validation across controllers.
public sealed class Course
{
    private static readonly HashSet<string> AllowedLevels =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Beginner",
            "Intermediate",
            "Advanced"
        };

    private Course(
        CourseId id,
        string title,
        string level,
        decimal price)
    {
        Id = id;
        Title = title;
        Level = level;
        Price = price;
    }

    public CourseId Id { get; }

    public string Title { get; private set; }

    public string Level { get; private set; }

    public decimal Price { get; private set; }

    public static Course Create(
        CourseId id,
        string title,
        string level,
        decimal price)
    {
        string cleanTitle = title.Trim();
        string cleanLevel = level.Trim();

        if (id.Value == Guid.Empty)
        {
            throw new DomainException("A course must have a non-empty ID.");
        }

        if (cleanTitle.Length is < 3 or > 120)
        {
            throw new DomainException("Course title must contain 3 to 120 characters.");
        }

        if (!AllowedLevels.Contains(cleanLevel))
        {
            throw new DomainException(
                "Course level must be Beginner, Intermediate, or Advanced.");
        }

        if (price < 0)
        {
            throw new DomainException("Course price cannot be negative.");
        }

        string canonicalLevel = AllowedLevels
            .First(levelName => string.Equals(
                levelName,
                cleanLevel,
                StringComparison.OrdinalIgnoreCase));

        return new Course(id, cleanTitle, canonicalLevel, decimal.Round(price, 2));
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Course price cannot be negative.");
        }

        Price = decimal.Round(price, 2);
    }
}
