using Audit.Application.Archiving;
using Audit.Infrastructure.Archiving.Execution;
using Audit.Infrastructure.Archiving.Reactivation;
using Audit.Infrastructure.Archiving.Transfers;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Archiving.Lifecycle;

public sealed class DatabaseContractArchiveLifecycle(
    AuditDbContext context,
    ContractArchivalExecutor archiver,
    ArchivedContractReactivator reactivator,
    ContractArchiveTransferStore transfers,
    TimeProvider timeProvider,
    ContractArchivalOptions options) : IContractArchiveLifecycle
{
    public async Task<ContractArchivalResult> RunOnceAsync(CancellationToken cancellationToken)
    {
        var pendingReactivation = await FindPendingReactivationAsync(cancellationToken);
        if (pendingReactivation is not null)
        {
            return await reactivator.ReactivateAsync(pendingReactivation, cancellationToken);
        }

        var archivalCandidate = await FindArchivalCandidateAsync(cancellationToken);
        if (archivalCandidate is null)
        {
            return new ContractArchivalResult(false, null, null, null, 0);
        }

        return await ArchiveContractAsync(archivalCandidate, cancellationToken);
    }

    private async Task<ContractArchiveTransferEntity?> FindPendingReactivationAsync(CancellationToken cancellationToken) =>
        await context.ContractArchiveTransfers
            .AsNoTracking()
            .Where(item => item.State == ArchiveTransferStates.ReactivationPending || item.State == ArchiveTransferStates.ReactivatedCopied)
            .OrderBy(item => item.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<ContractSearchEntity?> FindArchivalCandidateAsync(CancellationToken cancellationToken)
    {
        var policy = new ContractArchivalEligibilityPolicy(timeProvider, options.InactivityMonths);

        return await context.Contracts
            .AsNoTracking()
            .Where(contract => contract.LastActivityAt < policy.Cutoff)
            .Where(contract => !context.ContractArchiveTransfers.Any(transfer =>
                transfer.OrganizationId == contract.OrganizationId &&
                transfer.ContractId == contract.ContractId &&
                (transfer.State == ArchiveTransferStates.Archived ||
                 transfer.State == ArchiveTransferStates.ReactivationPending ||
                 transfer.State == ArchiveTransferStates.ReactivatedCopied)))
            .OrderBy(contract => contract.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ContractArchivalResult> ArchiveContractAsync(ContractSearchEntity candidate, CancellationToken cancellationToken)
    {
        try
        {
            return await archiver.ExecuteAsync(candidate, cancellationToken);
        }
        catch (Exception exception)
        {
            await transfers.MarkFailureAsync(candidate.OrganizationId, candidate.ContractId, exception, cancellationToken);
            throw;
        }
    }
}

