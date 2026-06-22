namespace Audit.Infrastructure.Persistence.Entities;

public sealed class ContractArchiveTransferEntity
{
    public Guid OrganizationId { get; set; }

    public Guid ContractId { get; set; }

    public required string State { get; set; }

    public DateTimeOffset LastActivityAt { get; set; }

    public long SnapshotSequence { get; set; }

    public int EventCount { get; set; }

    public int TimelineCount { get; set; }

    public string? ErrorCode { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
