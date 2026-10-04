namespace TestingLab;

public sealed class EnrollmentService(
    ICourseCatalog catalog,
    IEnrollmentStore store,
    IEnrollmentNotifier notifier,
    ISystemClock clock,
    IEnrollmentIdGenerator idGenerator)
{
    public async Task<EnrollmentReceipt> EnrollAsync(
        EnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Normalize once so the duplicate check, saved record, and receipt all
        // use the same canonical email value.
        string email = NormalizeEmail(request.StudentEmail);

        if (request.CourseId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.CourseId),
                "CourseId must be positive.");
        }

        // Look up the course through an interface. The unit test can replace
        // this with a deterministic fake instead of opening a database.
        Course? course = await catalog.FindAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            throw new CourseNotFoundException(request.CourseId);
        }

        // Check the business rule before creating any side effect.
        if (await store.ExistsAsync(email, course.Id, cancellationToken))
        {
            throw new EnrollmentConflictException(email, course.Id);
        }

        decimal price = EnrollmentPricing.Calculate(course.Price, request.EarlyBird);
        EnrollmentReceipt receipt = new(
            idGenerator.Create(),
            email,
            course.Title,
            price,
            clock.UtcNow);

        EnrollmentRecord record = new(
            receipt.EnrollmentId,
            receipt.StudentEmail,
            course.Id,
            receipt.Price,
            receipt.CreatedAtUtc);

        // Save first, then notify. A production system may use an outbox when
        // these two operations must be made durable together.
        await store.SaveAsync(record, cancellationToken);
        await notifier.SendConfirmationAsync(receipt, cancellationToken);

        return receipt;
    }

    private static string NormalizeEmail(string studentEmail)
    {
        string email = studentEmail.Trim().ToLowerInvariant();

        if (email.Length == 0 || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "StudentEmail must contain a value with an @ character.",
                nameof(studentEmail));
        }

        return email;
    }
}

public static class EnrollmentPricing
{
    public static decimal Calculate(decimal coursePrice, bool earlyBird)
    {
        if (coursePrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coursePrice),
                "Course price cannot be negative.");
        }

        // Keeping this pure makes the discount rule a fast, focused unit test.
        decimal finalPrice = earlyBird ? coursePrice * 0.90m : coursePrice;
        return decimal.Round(finalPrice, 2, MidpointRounding.AwayFromZero);
    }
}
