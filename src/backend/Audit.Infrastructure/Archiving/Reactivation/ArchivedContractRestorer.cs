using System.Data;
using Audit.Infrastructure.Archiving.Mapping;
using Audit.Infrastructure.Archiving.Queries;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Archiving.Reactivation;

public sealed class ArchivedContractRestorer(AuditDbContext context)
{
    public async Task RestoreAsync(
        ArchivedContractEntity archived,
        IReadOnlyCollection<ArchivedContractAliasEntity> aliases,
        IReadOnlyCollection<ArchivedAuditEventEntity> events,
        IReadOnlyCollection<ArchivedTimelineItemEntity> timeline,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archived);
        ArgumentNullException.ThrowIfNull(aliases);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(timeline);

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        await UpsertContractAsync(archived, cancellationToken);
        await AddMissingAliasesAsync(archived.OrganizationId, archived.ContractId, aliases, cancellationToken);
        await AddMissingAuditEventsAsync(events, cancellationToken);
        await AddMissingTimelineItemsAsync(archived.OrganizationId, archived.ContractId, timeline, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task UpsertContractAsync(ArchivedContractEntity archived, CancellationToken cancellationToken)
    {
        var current = await context.Contracts.SingleOrDefaultAsync(
            item => item.OrganizationId == archived.OrganizationId && item.ContractId == archived.ContractId,
            cancellationToken);

        if (current is null)
        {
            context.Contracts.Add(ArchiveEntityMapper.ToActive(archived));
            return;
        }

        if (current.LastSourceSequence < archived.LastSourceSequence)
        {
            current.Number = archived.Number;
            current.InternalNumber = archived.InternalNumber;
            current.Subject = archived.Subject;
            current.ContractorName = archived.ContractorName;
            current.LastSourceSequence = archived.LastSourceSequence;
        }

        if (current.LastActivityAt < archived.LastActivityAt)
        {
            current.LastActivityAt = archived.LastActivityAt;
        }
    }

    private async Task AddMissingAliasesAsync(
        Guid organizationId,
        Guid contractId,
        IReadOnlyCollection<ArchivedContractAliasEntity> aliases,
        CancellationToken cancellationToken)
    {
        var existingKeys = await LoadExistingAliasKeysAsync(organizationId, contractId, cancellationToken);

        var newAliases = aliases
            .DistinctBy(item => CreateAliasKey(item.Field, item.Value))
            .Where(item => !existingKeys.Contains(CreateAliasKey(item.Field, item.Value)))
            .Select(item => new ContractSearchAliasEntity
            {
                OrganizationId = item.OrganizationId,
                ContractId = item.ContractId,
                Field = item.Field,
                Value = item.Value,
                IsCurrent = item.IsCurrent
            });

        context.ContractAliases.AddRange(newAliases);
    }

    private async Task<HashSet<string>> LoadExistingAliasKeysAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var existing = await context.ContractAliases.AsNoTracking()
            .WhereContract(organizationId, contractId)
            .Select(item => new { item.Field, item.Value })
            .ToListAsync(cancellationToken);

        return existing
            .Select(item => CreateAliasKey(item.Field, item.Value))
            .ToHashSet(StringComparer.Ordinal);
    }

    private async Task AddMissingAuditEventsAsync(
        IReadOnlyCollection<ArchivedAuditEventEntity> events,
        CancellationToken cancellationToken)
    {
        var archivedIds = events.Select(item => item.EventId).ToArray();
        var existingIds = (await context.AuditEvents.AsNoTracking()
            .Where(item => archivedIds.Contains(item.EventId))
            .Select(item => item.EventId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        context.AuditEvents.AddRange(events
            .Where(item => !existingIds.Contains(item.EventId))
            .Select(ArchiveEntityMapper.ToActive));
    }

    private async Task AddMissingTimelineItemsAsync(
        Guid organizationId,
        Guid contractId,
        IReadOnlyCollection<ArchivedTimelineItemEntity> timeline,
        CancellationToken cancellationToken)
    {
        var existingIds = (await context.TimelineItems.AsNoTracking()
            .WhereContract(organizationId, contractId)
            .Select(item => item.EventId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        context.TimelineItems.AddRange(timeline
            .Where(item => !existingIds.Contains(item.EventId))
            .Select(ArchiveEntityMapper.ToActive));
    }

    private static string CreateAliasKey(string field, string value) => $"{field}\u001f{value}";
}
