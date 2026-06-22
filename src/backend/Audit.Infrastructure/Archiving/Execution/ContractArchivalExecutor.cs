using System.Data;
using Audit.Application.Archiving;
using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Archiving.Queries;
using Audit.Infrastructure.Archiving.Snapshots;
using Audit.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Execution;

public sealed class ContractArchivalExecutor(
    AuditDbContext context,
    ContractArchiveSnapshotStore snapshots,
    TimeProvider timeProvider)
{
    private const string ActionArchived = "Archived";
    private const string ActionCopyCancelled = "CopyCancelledBecauseContractChanged";

    public async Task<ContractArchivalResult> ExecuteAsync(
        ContractSearchEntity contract,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contract);

        var archiveData = await LoadContractDataAsync(contract, cancellationToken);
        var transfer = await UpsertTransferToCopyingAsync(contract, archiveData, cancellationToken);
        await CreateAndVerifySnapshotAsync(contract, transfer, archiveData, cancellationToken);
        return await CommitArchivalAsync(contract, transfer, archiveData.Events.Count, cancellationToken);
    }

    private async Task<ContractArchiveData> LoadContractDataAsync(
        ContractSearchEntity contract,
        CancellationToken cancellationToken)
    {
        var aliases = await context.ContractAliases.AsNoTracking()
            .WhereContract(contract.OrganizationId, contract.ContractId)
            .ToListAsync(cancellationToken);

        var timeline = await context.TimelineItems.AsNoTracking()
            .WhereContract(contract.OrganizationId, contract.ContractId)
            .OrderBy(item => item.SourceSequence)
            .ToListAsync(cancellationToken);

        if (timeline.Count == 0)
        {
            throw new InvalidOperationException("archiveContractHasNoTimeline");
        }

        var eventIds = timeline.Select(item => item.EventId).ToArray();
        var events = await context.AuditEvents.AsNoTracking()
            .Where(item => eventIds.Contains(item.EventId))
            .OrderBy(item => item.SourceSequence)
            .ToListAsync(cancellationToken);

        if (events.Any(item => item.OrganizationId is null))
        {
            throw new InvalidOperationException("archiveOrganizationMissing");
        }

        return new ContractArchiveData(aliases, timeline, events);
    }

    private async Task<ContractArchiveTransferEntity> UpsertTransferToCopyingAsync(
        ContractSearchEntity contract,
        ContractArchiveData archiveData,
        CancellationToken cancellationToken)
    {
        var snapshotSequence = archiveData.Timeline.Max(item => item.SourceSequence);
        var now = timeProvider.GetUtcNow();

        var transfer = await context.ContractArchiveTransfers.FindAsync(
            [contract.OrganizationId, contract.ContractId],
            cancellationToken);

        if (transfer is null)
        {
            transfer = new ContractArchiveTransferEntity
            {
                OrganizationId = contract.OrganizationId,
                ContractId = contract.ContractId,
                State = ArchiveTransferStates.Copying,
                StartedAt = now,
                UpdatedAt = now
            };
            context.ContractArchiveTransfers.Add(transfer);
        }

        transfer.State = ArchiveTransferStates.Copying;
        transfer.LastActivityAt = contract.LastActivityAt;
        transfer.SnapshotSequence = snapshotSequence;
        transfer.EventCount = archiveData.Events.Count;
        transfer.TimelineCount = archiveData.Timeline.Count;
        transfer.ErrorCode = null;
        transfer.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        return transfer;
    }

    private async Task CreateAndVerifySnapshotAsync(
        ContractSearchEntity contract,
        ContractArchiveTransferEntity transfer,
        ContractArchiveData archiveData,
        CancellationToken cancellationToken)
    {
        await snapshots.ReplaceAsync(
            contract,
            archiveData.Aliases,
            archiveData.Events,
            archiveData.Timeline,
            transfer.UpdatedAt,
            cancellationToken);
        await snapshots.VerifySnapshotIntegrityAsync(transfer, cancellationToken);

        transfer.State = ArchiveTransferStates.Verified;
        transfer.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ContractArchivalResult> CommitArchivalAsync(
        ContractSearchEntity contract,
        ContractArchiveTransferEntity transfer,
        int eventCount,
        CancellationToken cancellationToken)
    {
        // The ChangeTracker must be cleared before the commit step. Snapshot verification loads entities
        // into the tracker; those share primary keys with the entities about to be deleted here.
        // EF Core would attempt to track both sets simultaneously, causing a duplicate-key conflict
        // on SaveChangesAsync. Clearing the tracker prevents that conflict.
        context.ChangeTracker.Clear();

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var current = await context.Contracts.SingleOrDefaultAsync(
            item => item.OrganizationId == contract.OrganizationId && item.ContractId == contract.ContractId,
            cancellationToken);

        var currentMetrics = await LoadActiveSnapshotMetricsAsync(
            contract.OrganizationId,
            contract.ContractId,
            cancellationToken);

        if (!IsConsistentWithSnapshot(current, currentMetrics, transfer))
        {
            await transaction.RollbackAsync(cancellationToken);
            await snapshots.DeleteAsync(contract.OrganizationId, contract.ContractId, cancellationToken);
            await ResetTransferToActiveAsync(contract.OrganizationId, contract.ContractId, cancellationToken);
            return new ContractArchivalResult(
                true,
                contract.OrganizationId,
                contract.ContractId,
                ActionCopyCancelled,
                eventCount);
        }

        await DeleteContractSourceDataAsync(contract, cancellationToken);
        context.Contracts.Remove(current!);

        var currentTransfer = await context.ContractArchiveTransfers.FindAsync(
            [contract.OrganizationId, contract.ContractId],
            cancellationToken) ?? throw new InvalidOperationException("archiveTransferMissing");
        currentTransfer.State = ArchiveTransferStates.Archived;
        currentTransfer.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ContractArchivalResult(true, contract.OrganizationId, contract.ContractId, ActionArchived, eventCount);
    }

    private static bool IsConsistentWithSnapshot(
        ContractSearchEntity? current,
        ContractArchiveSnapshotMetrics? currentMetrics,
        ContractArchiveTransferEntity transfer) =>
        current is not null
        && current.LastActivityAt == transfer.LastActivityAt
        && currentMetrics?.Matches(transfer) == true;

    private async Task<ContractArchiveSnapshotMetrics?> LoadActiveSnapshotMetricsAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken) =>
        await context.TimelineItems
            .WhereContract(organizationId, contractId)
            .GroupBy(_ => 1)
            .Select(group => new ContractArchiveSnapshotMetrics(
                group.Count(),
                group.Max(item => item.SourceSequence)))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task DeleteContractSourceDataAsync(
        ContractSearchEntity contract,
        CancellationToken cancellationToken)
    {
        // Raw SQL is required because EF Core does not support DELETE with a JOIN. This statement
        // removes audit_events whose EventIds are referenced by contract_timeline_items for this
        // contract — a cross-table deletion that cannot be expressed as a single ExecuteDeleteAsync call.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DELETE audit
            FROM audit_events audit
            INNER JOIN contract_timeline_items timeline ON timeline.EventId = audit.EventId
            WHERE timeline.OrganizationId = {contract.OrganizationId}
              AND timeline.ContractId = {contract.ContractId};
            """,
            cancellationToken);
        await context.TimelineItems
            .WhereContract(contract.OrganizationId, contract.ContractId)
            .ExecuteDeleteAsync(cancellationToken);
        await context.ContractAliases
            .WhereContract(contract.OrganizationId, contract.ContractId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task ResetTransferToActiveAsync(
        Guid organizationId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        var transfer = await context.ContractArchiveTransfers.FindAsync(
            [organizationId, contractId],
            cancellationToken) ?? throw new InvalidOperationException("archiveTransferMissing");
        transfer.State = ArchiveTransferStates.Active;
        transfer.ErrorCode = null;
        transfer.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record ContractArchiveData(
        List<ContractSearchAliasEntity> Aliases,
        List<ContractTimelineItemEntity> Timeline,
        List<CanonicalAuditEventEntity> Events);
}

