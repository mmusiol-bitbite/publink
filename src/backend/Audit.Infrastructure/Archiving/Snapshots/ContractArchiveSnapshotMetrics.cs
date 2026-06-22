using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Snapshots;

internal sealed record ContractArchiveSnapshotMetrics(
    int TimelineCount,
    long SnapshotSequence,
    int? EventCount = null)
{
    public bool Matches(ContractArchiveTransferEntity transfer) =>
        TimelineCount == transfer.TimelineCount
        && SnapshotSequence == transfer.SnapshotSequence
        && (EventCount is null || EventCount == transfer.EventCount);
}
