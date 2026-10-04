namespace TestingLab;

public sealed record Course(
    int Id,
    string Title,
    decimal Price);

public sealed record EnrollmentRequest(
    string StudentEmail,
    int CourseId,
    bool EarlyBird);

public sealed record EnrollmentRecord(
    Guid Id,
    string StudentEmail,
    int CourseId,
    decimal Price,
    DateTimeOffset CreatedAtUtc);

public sealed record EnrollmentReceipt(
    Guid EnrollmentId,
    string StudentEmail,
    string CourseTitle,
    decimal Price,
    DateTimeOffset CreatedAtUtc);
