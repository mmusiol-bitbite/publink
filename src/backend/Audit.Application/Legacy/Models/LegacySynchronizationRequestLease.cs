namespace Audit.Application.Legacy;

public sealed record LegacySynchronizationRequestLease(
    Guid CorrelationId,
    string Source,
    DateTimeOffset RequestedAt,
    bool Created);
