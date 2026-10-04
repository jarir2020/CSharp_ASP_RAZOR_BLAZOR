using TestingLab;
using Xunit;

namespace CSharpCore.Tests;

// A fixture holds stable scenario setup. Each test still receives fresh
// collaborators from CreateContext, so state cannot leak between test cases.
public sealed class EnrollmentScenarioFixture
{
    public Course Course { get; } = new(7, "Testing ASP.NET Core", 125m);

    public EnrollmentContext CreateContext(bool alreadyEnrolled = false)
    {
        FakeCourseCatalog catalog = new(Course);
        FakeEnrollmentStore store = new(alreadyEnrolled);
        FakeNotifier notifier = new();
        FixedClock clock = new(new DateTimeOffset(2026, 10, 5, 0, 0, 0, TimeSpan.Zero));
        FixedIdGenerator ids = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        EnrollmentService service = new(catalog, store, notifier, clock, ids);
        return new(service, store, notifier);
    }
}

public sealed record EnrollmentContext(
    EnrollmentService Service,
    FakeEnrollmentStore Store,
    FakeNotifier Notifier);

public sealed class EnrollmentServiceUnitTests : IClassFixture<EnrollmentScenarioFixture>
{
    private readonly EnrollmentScenarioFixture _fixture;

    public EnrollmentServiceUnitTests(EnrollmentScenarioFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Successful_enrollment_normalizes_email_applies_discount_and_notifies()
    {
        EnrollmentContext context = _fixture.CreateContext();

        EnrollmentReceipt receipt = await context.Service.EnrollAsync(
            new EnrollmentRequest("  LEARNER@EXAMPLE.COM ", 7, EarlyBird: true));

        Assert.Equal("learner@example.com", receipt.StudentEmail);
        Assert.Equal(112.50m, receipt.Price);
        Assert.Equal(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), receipt.EnrollmentId);
        Assert.Single(context.Store.Saved);
        Assert.Equal(receipt, context.Notifier.LastReceipt);
    }

    [Fact]
    public async Task Duplicate_enrollment_throws_before_writing_or_notifying()
    {
        EnrollmentContext context = _fixture.CreateContext(alreadyEnrolled: true);

        await Assert.ThrowsAsync<EnrollmentConflictException>(() =>
            context.Service.EnrollAsync(
                new EnrollmentRequest("learner@example.com", 7, EarlyBird: false)));

        Assert.Empty(context.Store.Saved);
        Assert.Null(context.Notifier.LastReceipt);
    }

    [Fact]
    public async Task Unknown_course_throws_before_duplicate_check()
    {
        EnrollmentContext context = _fixture.CreateContext();

        CourseNotFoundException exception = await Assert.ThrowsAsync<CourseNotFoundException>(() =>
            context.Service.EnrollAsync(
                new EnrollmentRequest("learner@example.com", 99, EarlyBird: false)));

        Assert.Equal(99, exception.CourseId);
        Assert.Empty(context.Store.Saved);
        Assert.Null(context.Notifier.LastReceipt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("missing-at-symbol")]
    public async Task Invalid_email_is_rejected_before_catalog_access(string email)
    {
        EnrollmentContext context = _fixture.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.Service.EnrollAsync(
                new EnrollmentRequest(email, 7, EarlyBird: false)));

        Assert.Empty(context.Store.Saved);
        Assert.Null(context.Notifier.LastReceipt);
    }

    [Fact]
    public void Pricing_rounds_the_early_bird_discount_deterministically()
    {
        Assert.Equal(89.99m, EnrollmentPricing.Calculate(99.99m, earlyBird: true));
        Assert.Equal(99.99m, EnrollmentPricing.Calculate(99.99m, earlyBird: false));
    }
}

// These fakes are intentionally boring. Their state makes collaborator
// interactions visible in a debugger and keeps this unit test independent of
// a database, network, clock, and random number generator.
public sealed class FakeCourseCatalog(Course course) : ICourseCatalog
{
    public Task<Course?> FindAsync(int courseId, CancellationToken cancellationToken)
    {
        Course? result = course.Id == courseId ? course : null;
        return Task.FromResult(result);
    }
}

public sealed class FakeEnrollmentStore(bool alreadyEnrolled) : IEnrollmentStore
{
    private readonly bool _alreadyEnrolled = alreadyEnrolled;

    public List<EnrollmentRecord> Saved { get; } = [];

    public Task<bool> ExistsAsync(
        string studentEmail,
        int courseId,
        CancellationToken cancellationToken)
    {
        bool exists = _alreadyEnrolled || Saved.Any(record =>
            record.StudentEmail == studentEmail && record.CourseId == courseId);
        return Task.FromResult(exists);
    }

    public Task SaveAsync(
        EnrollmentRecord enrollment,
        CancellationToken cancellationToken)
    {
        Saved.Add(enrollment);
        return Task.CompletedTask;
    }
}

public sealed class FakeNotifier : IEnrollmentNotifier
{
    public EnrollmentReceipt? LastReceipt { get; private set; }

    public Task SendConfirmationAsync(
        EnrollmentReceipt receipt,
        CancellationToken cancellationToken)
    {
        LastReceipt = receipt;
        return Task.CompletedTask;
    }
}

public sealed class FixedClock(DateTimeOffset value) : ISystemClock
{
    public DateTimeOffset UtcNow => value;
}

public sealed class FixedIdGenerator(Guid value) : IEnrollmentIdGenerator
{
    public Guid Create() => value;
}
