namespace ProductionLab.Models;

public sealed record AuditEvent(
    DateTimeOffset CreatedAtUtc,
    string CorrelationId,
    string Method,
    string Path,
    int StatusCode,
    long ElapsedMilliseconds);
