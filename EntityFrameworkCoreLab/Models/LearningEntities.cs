namespace EntityFrameworkCoreLab.Models;

public enum EnrollmentStatus
{
    Active,
    Completed,
    Cancelled
}

public sealed class Course
{
    public int CourseId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public CourseSettings? Settings { get; set; }

    // A course has many enrollment rows.
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public sealed class CourseSettings
{
    // This is both the primary key and the foreign key to Course, which models
    // a one-to-one relationship.
    public int CourseId { get; set; }

    public string Difficulty { get; set; } = string.Empty;

    public int EstimatedHours { get; set; }

    public Course Course { get; set; } = null!;
}

public sealed class Student
{
    public int StudentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // A student has many enrollment rows.
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public sealed class Enrollment
{
    // The pair StudentId + CourseId is configured as a composite key. This
    // prevents the same student enrolling in the same course twice.
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public DateTime EnrolledAtUtc { get; set; }

    public EnrollmentStatus Status { get; set; }
}
