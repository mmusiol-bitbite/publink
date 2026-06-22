namespace Audit.Application.Queries;

public sealed record TimelinePage(
    Guid ContractId,
    long SnapshotSequence,
    DateTimeOffset? SynchronizedAt,
    IReadOnlyList<TimelineItem> Items,
    string? NextCursor);
