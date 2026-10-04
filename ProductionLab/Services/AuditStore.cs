using System.Collections.Concurrent;
using ProductionLab.Models;

namespace ProductionLab.Services;

public sealed class AuditStore
{
    private const int MaximumItems = 100;
    private readonly ConcurrentQueue<AuditEvent> _events = new();

    public void Add(AuditEvent auditEvent)
    {
        _events.Enqueue(auditEvent);

        // This is a bounded diagnostic buffer, not a durable audit database.
        while (_events.Count > MaximumItems && _events.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<AuditEvent> Latest(int count)
    {
        int safeCount = Math.Clamp(count, 1, MaximumItems);
        return _events
            .Reverse()
            .Take(safeCount)
            .ToList();
    }
}
