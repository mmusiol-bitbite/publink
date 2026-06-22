using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Mapping;

internal static class ArchiveEntityMapper
{
    public static ArchivedContractEntity ToArchive(ContractSearchEntity source, DateTimeOffset archivedAt) => new()
    {
        OrganizationId = source.OrganizationId,
        ContractId = source.ContractId,
        Number = source.Number,
        InternalNumber = source.InternalNumber,
        Subject = source.Subject,
        ContractorName = source.ContractorName,
        LastSourceSequence = source.LastSourceSequence,
        LastActivityAt = source.LastActivityAt,
        ArchivedAt = archivedAt
    };

    public static ArchivedContractAliasEntity ToArchive(ContractSearchAliasEntity source) => new()
    {
        OrganizationId = source.OrganizationId,
        ContractId = source.ContractId,
        Field = source.Field,
        Value = source.Value,
        IsCurrent = source.IsCurrent
    };

    public static ArchivedAuditEventEntity ToArchive(
        CanonicalAuditEventEntity item,
        Guid contractId) => new()
        {
            EventId = item.EventId,
            OrganizationId = item.OrganizationId!.Value,
            ContractId = contractId,
            Source = item.Source,
            SourceEventId = item.SourceEventId,
            SourceSequence = item.SourceSequence,
            OccurredAt = item.OccurredAt,
            IngestedAt = item.IngestedAt,
            ActorId = item.ActorId,
            ActorEmail = item.ActorEmail,
            CorrelationId = item.CorrelationId,
            ChangeTypeCode = item.ChangeTypeCode,
            EntityTypeCode = item.EntityTypeCode,
            EntityId = item.EntityId,
            ParentId = item.ParentId,
            PrimaryKey = item.PrimaryKey,
            BeforeJson = item.BeforeJson,
            AfterJson = item.AfterJson,
            ChangedFieldsJson = item.ChangedFieldsJson
        };

    public static CanonicalAuditEventEntity ToActive(ArchivedAuditEventEntity item) => new()
    {
        EventId = item.EventId,
        Source = item.Source,
        SourceEventId = item.SourceEventId,
        SourceSequence = item.SourceSequence,
        OrganizationId = item.OrganizationId,
        OccurredAt = item.OccurredAt,
        IngestedAt = item.IngestedAt,
        ActorId = item.ActorId,
        ActorEmail = item.ActorEmail,
        CorrelationId = item.CorrelationId,
        ChangeTypeCode = item.ChangeTypeCode,
        EntityTypeCode = item.EntityTypeCode,
        EntityId = item.EntityId,
        ParentId = item.ParentId,
        PrimaryKey = item.PrimaryKey,
        BeforeJson = item.BeforeJson,
        AfterJson = item.AfterJson,
        ChangedFieldsJson = item.ChangedFieldsJson
    };

    public static ArchivedTimelineItemEntity ToArchive(ContractTimelineItemEntity item) => new()
    {
        EventId = item.EventId,
        OrganizationId = item.OrganizationId
            ?? throw new InvalidOperationException("archiveOrganizationMissing"),
        ContractId = item.ContractId
            ?? throw new InvalidOperationException("archiveContractMissing"),
        SourceSequence = item.SourceSequence,
        OccurredAt = item.OccurredAt,
        CorrelationId = item.CorrelationId,
        ChangeTypeCode = item.ChangeTypeCode,
        ChangeKind = item.ChangeKind,
        EntityTypeCode = item.EntityTypeCode,
        EntityKind = item.EntityKind,
        Actor = item.Actor,
        ChangesJson = item.ChangesJson,
        DataQualityIssuesJson = item.DataQualityIssuesJson
    };

    public static ContractTimelineItemEntity ToActive(ArchivedTimelineItemEntity item) => new()
    {
        EventId = item.EventId,
        OrganizationId = item.OrganizationId,
        ContractId = item.ContractId,
        ContractIdResolved = true,
        SourceSequence = item.SourceSequence,
        OccurredAt = item.OccurredAt,
        CorrelationId = item.CorrelationId,
        ChangeTypeCode = item.ChangeTypeCode,
        ChangeKind = item.ChangeKind,
        EntityTypeCode = item.EntityTypeCode,
        EntityKind = item.EntityKind,
        Actor = item.Actor,
        ChangesJson = item.ChangesJson,
        DataQualityIssuesJson = item.DataQualityIssuesJson
    };

    public static ContractSearchEntity ToActive(ArchivedContractEntity archived) => new()
    {
        OrganizationId = archived.OrganizationId,
        ContractId = archived.ContractId,
        Number = archived.Number,
        InternalNumber = archived.InternalNumber,
        Subject = archived.Subject,
        ContractorName = archived.ContractorName,
        LastSourceSequence = archived.LastSourceSequence,
        LastActivityAt = archived.LastActivityAt
    };
}

