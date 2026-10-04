using System.Collections.Concurrent;
using AdvancedAspNetCoreLab.Models;

namespace AdvancedAspNetCoreLab.Background;

public sealed class JobStore
{
    private readonly ConcurrentDictionary<Guid, JobStatus> _jobs = new();

    public Guid Create()
    {
        Guid id = Guid.NewGuid();
        _jobs[id] = new JobStatus(id, JobState.Pending, DateTimeOffset.UtcNow, null, null);
        return id;
    }

    public bool TryGet(Guid id, out JobStatus? status)
    {
        return _jobs.TryGetValue(id, out status);
    }

    public void MarkRunning(Guid id)
    {
        Update(id, JobState.Running, null);
    }

    public void MarkCompleted(Guid id)
    {
        Update(id, JobState.Completed, null);
    }

    public void MarkFailed(Guid id, string error)
    {
        Update(id, JobState.Failed, error);
    }

    private void Update(Guid id, JobState state, string? error)
    {
        _jobs.AddOrUpdate(
            id,
            _ => new JobStatus(id, state, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, error),
            (_, existing) => existing with
            {
                State = state,
                FinishedAtUtc = state is JobState.Completed or JobState.Failed
                    ? DateTimeOffset.UtcNow
                    : null,
                Error = error
            });
    }
}
