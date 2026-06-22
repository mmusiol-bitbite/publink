namespace Audit.Infrastructure.Persistence.Entities;

public sealed class ArchivedTimelineItemEntity
{
    public Guid EventId { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ContractId { get; set; }

    public long SourceSequence { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public Guid CorrelationId { get; set; }

    public int ChangeTypeCode { get; set; }

    public required string ChangeKind { get; set; }

    public int EntityTypeCode { get; set; }

    public required string EntityKind { get; set; }

    public required string Actor { get; set; }

    public required string ChangesJson { get; set; }

    public required string DataQualityIssuesJson { get; set; }
}
