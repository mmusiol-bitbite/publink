namespace Audit.Infrastructure.Persistence.Entities;

public sealed class CanonicalAuditEventEntity
{
    public Guid EventId { get; set; }

    public required string Source { get; set; }

    public long SourceEventId { get; set; }

    public long SourceSequence { get; set; }

    public Guid? OrganizationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset IngestedAt { get; set; }

    public Guid? ActorId { get; set; }

    public string? ActorEmail { get; set; }

    public Guid CorrelationId { get; set; }

    public int ChangeTypeCode { get; set; }

    public int EntityTypeCode { get; set; }

    public Guid? EntityId { get; set; }

    public Guid? ParentId { get; set; }

    public string? PrimaryKey { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public required string ChangedFieldsJson { get; set; }
}

