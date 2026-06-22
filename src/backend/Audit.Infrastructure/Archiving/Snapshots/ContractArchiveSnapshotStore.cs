using Audit.Infrastructure.Archiving.Mapping;
using Audit.Infrastructure.Archiving.Queries;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Archiving.Snapshots;

public sealed class ContractArchiveSnapshotStore(ArchiveDbContext dbContext)
{
    public async Task<ArchivedContractSnapshot?> LoadAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await dbContext.Contracts.AsNoTracking().SingleOrDefaultAsync(
            item => item.OrganizationId == organizationId && item.ContractId == contractId,
            cancellationToken);
        if (contract is null)
        {
            return null;
        }

        var aliases = await dbContext.ContractAliases.AsNoTracking()
            .WhereContract(organizationId, contractId)
            .ToListAsync(cancellationToken);

        var events = await dbContext.AuditEvents.AsNoTracking()
            .WhereContract(organizationId, contractId)
            .ToListAsync(cancellationToken);

        var timeline = await dbContext.TimelineItems.AsNoTracking()
            .WhereContract(organizationId, contractId)
            .ToListAsync(cancellationToken);

        return new ArchivedContractSnapshot(contract, aliases, events, timeline);
    }

    public async Task ReplaceAsync(
        ContractSearchEntity contract,
        IReadOnlyCollection<ContractSearchAliasEntity> aliases,
        IReadOnlyCollection<CanonicalAuditEventEntity> events,
        IReadOnlyCollection<ContractTimelineItemEntity> timeline,
        DateTimeOffset archivedAt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(aliases);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(timeline);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await DeleteCoreAsync(contract.OrganizationId, contract.ContractId, cancellationToken);
        await InsertSnapshotAsync(contract, aliases, events, timeline, archivedAt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // InsertSnapshotAsync tracks every inserted entity as Unchanged after SaveChangesAsync.
        // Clear here so callers (e.g. VerifyAsync, DeleteAsync) work on a clean tracker
        // and DeleteAsync does not need to know what this method did.
        dbContext.ChangeTracker.Clear();
    }

    private async Task InsertSnapshotAsync(
        ContractSearchEntity contract,
        IReadOnlyCollection<ContractSearchAliasEntity> aliases,
        IReadOnlyCollection<CanonicalAuditEventEntity> events,
        IReadOnlyCollection<ContractTimelineItemEntity> timeline,
        DateTimeOffset archivedAt,
        CancellationToken cancellationToken)
    {
        dbContext.Contracts.Add(ArchiveEntityMapper.ToArchive(contract, archivedAt));
        dbContext.ContractAliases.AddRange(aliases.Select(ArchiveEntityMapper.ToArchive));
        dbContext.AuditEvents.AddRange(events.Select(item => ArchiveEntityMapper.ToArchive(item, contract.ContractId)));
        dbContext.TimelineItems.AddRange(timeline.Select(ArchiveEntityMapper.ToArchive));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifySnapshotIntegrityAsync(
        ContractArchiveTransferEntity transfer,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        var eventCount = await dbContext.AuditEvents.CountAsync(
            item => item.OrganizationId == transfer.OrganizationId && item.ContractId == transfer.ContractId,
            cancellationToken);

        var metrics = await LoadSnapshotMetricsAsync(
            transfer.OrganizationId,
            transfer.ContractId,
            eventCount,
            cancellationToken);

        if (metrics?.Matches(transfer) != true)
        {
            throw new InvalidOperationException("archiveVerificationFailed");
        }
    }

    private async Task<ContractArchiveSnapshotMetrics?> LoadSnapshotMetricsAsync(
        Guid organizationId,
        Guid contractId,
        int eventCount,
        CancellationToken cancellationToken) =>
        await dbContext.TimelineItems
            .WhereContract(organizationId, contractId)
            .GroupBy(_ => 1)
            .Select(group => new ContractArchiveSnapshotMetrics(
                group.Count(),
                group.Max(item => item.SourceSequence),
                eventCount))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task DeleteAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await DeleteCoreAsync(organizationId, contractId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task DeleteCoreAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        await dbContext.AuditEvents
            .WhereContract(organizationId, contractId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.TimelineItems
            .WhereContract(organizationId, contractId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.ContractAliases
            .WhereContract(organizationId, contractId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Contracts
            .WhereContract(organizationId, contractId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}


