namespace TestingLab;

// These small interfaces are seams. The service can depend on behavior while
// each test supplies a deterministic implementation of that behavior.
public interface ICourseCatalog
{
    Task<Course?> FindAsync(int courseId, CancellationToken cancellationToken);
}

public interface IEnrollmentStore
{
    Task<bool> ExistsAsync(
        string studentEmail,
        int courseId,
        CancellationToken cancellationToken);

    Task SaveAsync(
        EnrollmentRecord enrollment,
        CancellationToken cancellationToken);
}

public interface IEnrollmentNotifier
{
    Task SendConfirmationAsync(
        EnrollmentReceipt receipt,
        CancellationToken cancellationToken);
}

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IEnrollmentIdGenerator
{
    Guid Create();
}
