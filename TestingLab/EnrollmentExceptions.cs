namespace TestingLab;

public sealed class CourseNotFoundException(int courseId)
    : Exception($"Course {courseId} was not found.")
{
    public int CourseId { get; } = courseId;
}

public sealed class EnrollmentConflictException(string studentEmail, int courseId)
    : Exception($"Student {studentEmail} is already enrolled in course {courseId}.")
{
    public string StudentEmail { get; } = studentEmail;

    public int CourseId { get; } = courseId;
}
