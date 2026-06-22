using Audit.Application.Persistence;
using Audit.Contracts;
using Audit.Domain;
using Audit.Infrastructure.Persistence.Contexts;

namespace Audit.Infrastructure.Persistence.Persisters;

public sealed class ContractProjectionPersister(
    AuditDbContext dbContext,
    ContractSearchProjectionWriter searchProjectionWriter)
    : IContractProjectionPersister
{
    public async Task ApplyAsync(
        AuditEntryImportedV1 message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var contractId = ContractTimelineItemMapper.ResolveContractId(message);
        dbContext.TimelineItems.Add(ContractTimelineItemMapper.Map(message, contractId));

        if (message.Entity.EntityTypeCode == AuditedEntityKind.ContractHeader.Code &&
            message.Entity.EntityId is { } entityId &&
            message.OrganizationId is { } organizationId)
        {
            await searchProjectionWriter.UpdateContractAsync(
                organizationId,
                entityId,
                message,
                cancellationToken);
        }

        if (contractId is { } resolvedContractId &&
            message.OrganizationId is { } resolvedOrganizationId)
        {
            await searchProjectionWriter.TrackActivityAsync(
                resolvedOrganizationId,
                resolvedContractId,
                message.OccurredAt,
                cancellationToken);
        }
    }
}
