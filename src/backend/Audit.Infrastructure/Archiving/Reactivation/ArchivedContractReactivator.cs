using Audit.Application.Archiving;
using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Archiving.Snapshots;
using Audit.Infrastructure.Archiving.Transfers;
using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Reactivation;

public sealed class ArchivedContractReactivator(
    ContractArchiveSnapshotStore snapshots,
    ArchivedContractRestorer restorer,
    ContractArchiveTransferStore transfers)
{
    private const string ActionReactivated = "Reactivated";
    private const string ActionReactivationRecovered = "ReactivationRecovered";

    public async Task<ContractArchivalResult> ReactivateAsync(
        ContractArchiveTransferEntity transfer,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        var snapshot = await snapshots.LoadAsync(transfer.OrganizationId, transfer.ContractId, cancellationToken);

        if (snapshot is null)
        {
            await transfers.SetStateAsync(transfer.OrganizationId, transfer.ContractId, ArchiveTransferStates.Active, cancellationToken);
            return new ContractArchivalResult(true, transfer.OrganizationId, transfer.ContractId, ActionReactivationRecovered, 0);
        }

        if (transfer.State == ArchiveTransferStates.ReactivationPending)
        {
            await restorer.RestoreAsync(snapshot.Contract, snapshot.Aliases, snapshot.Events, snapshot.Timeline, cancellationToken);
            await transfers.SetStateAsync(transfer.OrganizationId, transfer.ContractId, ArchiveTransferStates.ReactivatedCopied, cancellationToken);
        }

        await snapshots.DeleteAsync(transfer.OrganizationId, transfer.ContractId, cancellationToken);
        await transfers.SetStateAsync(transfer.OrganizationId, transfer.ContractId, ArchiveTransferStates.Active, cancellationToken);

        return new ContractArchivalResult(true, transfer.OrganizationId, transfer.ContractId, ActionReactivated, transfer.EventCount);
    }
}
