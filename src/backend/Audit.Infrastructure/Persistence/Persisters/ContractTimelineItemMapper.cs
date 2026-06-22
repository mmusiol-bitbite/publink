using System.Text.Json;
using Audit.Contracts;
using Audit.Domain;
using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Persistence.Persisters;

internal static class ContractTimelineItemMapper
{
    public static Guid? ResolveContractId(AuditEntryImportedV1 message) =>
        message.Entity.EntityTypeCode == AuditedEntityKind.ContractHeader.Code
            ? message.Entity.EntityId
            : message.Entity.ParentId;

    public static ContractTimelineItemEntity Map(
        AuditEntryImportedV1 message,
        Guid? contractId)
    {
        var changeSet = FieldChangeFactory.Create(
            message.Before?.GetRawText(),
            message.After?.GetRawText(),
            message.ChangedFields);
        return new ContractTimelineItemEntity
        {
            EventId = message.EventId,
            OrganizationId = message.OrganizationId,
            ContractId = contractId,
            ContractIdResolved = contractId.HasValue,
            SourceSequence = message.SourceSequence,
            OccurredAt = message.OccurredAt,
            CorrelationId = message.Operation.CorrelationId,
            ChangeTypeCode = message.Operation.ChangeTypeCode,
            ChangeKind = ChangeKind.FromCode(message.Operation.ChangeTypeCode).Key,
            EntityTypeCode = message.Entity.EntityTypeCode,
            EntityKind = AuditedEntityKind.FromCode(message.Entity.EntityTypeCode).Key,
            Actor = message.Actor.Email ?? message.Actor.UserId?.ToString() ?? "system-or-unknown",
            ChangesJson = JsonSerializer.Serialize(changeSet.Changes),
            DataQualityIssuesJson = JsonSerializer.Serialize(BuildIssues(message, changeSet, contractId))
        };
    }

    private static List<string> BuildIssues(
        AuditEntryImportedV1 message,
        ChangeSet changeSet,
        Guid? contractId)
    {
        var issues = new List<string>(changeSet.DataQualityIssues);
        if (!contractId.HasValue)
        {
            issues.Add("unresolvedContractRelationship");
        }

        if (ChangeKind.FromCode(message.Operation.ChangeTypeCode).Key == "unknown")
        {
            issues.Add("unknownChangeType");
        }

        if (AuditedEntityKind.FromCode(message.Entity.EntityTypeCode).Key == "unknown")
        {
            issues.Add("unknownEntityType");
        }

        return issues;
    }
}
