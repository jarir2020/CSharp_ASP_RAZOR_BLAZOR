namespace AdvancedAspNetCoreLab.Models;

public enum JobState
{
    Pending,
    Running,
    Completed,
    Failed
}

public sealed record JobStatus(
    Guid Id,
    JobState State,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? FinishedAtUtc,
    string? Error);
