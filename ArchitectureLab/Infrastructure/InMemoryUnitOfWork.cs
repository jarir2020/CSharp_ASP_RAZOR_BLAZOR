using ArchitectureLab.Application;

namespace ArchitectureLab.Infrastructure;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public int CommitCount { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        // A database-backed implementation would commit a transaction here.
        CommitCount++;
        return Task.CompletedTask;
    }
}
