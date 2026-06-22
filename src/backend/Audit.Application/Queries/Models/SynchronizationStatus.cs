namespace Audit.Application.Queries;

public sealed record SynchronizationStatus(
    string Source,
    long LastSourceEventId,
    DateTimeOffset? LastSynchronizedAt,
    string Status,
    long? DeadLetterEventCount,
    Guid? ActiveRequestId,
    DateTimeOffset? ActiveRequestAcceptedAt);
