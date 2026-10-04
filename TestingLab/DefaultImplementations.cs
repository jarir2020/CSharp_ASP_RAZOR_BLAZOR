namespace TestingLab;

public sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class GuidEnrollmentIdGenerator : IEnrollmentIdGenerator
{
    public Guid Create() => Guid.NewGuid();
}
