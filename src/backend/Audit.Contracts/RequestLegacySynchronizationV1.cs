namespace Audit.Contracts;

public sealed record RequestLegacySynchronizationV1(
    Guid CorrelationId,
    string Source,
    DateTimeOffset RequestedAt,
    int SchemaVersion = 1) : AuditContract;
