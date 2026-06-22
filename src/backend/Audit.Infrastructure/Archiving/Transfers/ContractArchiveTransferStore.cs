using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Transfers;

public sealed class ContractArchiveTransferStore(
    AuditDbContext context,
    TimeProvider timeProvider)
{
    public Task SetStateAsync(
        Guid organizationId,
        Guid contractId,
        string state,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            organizationId,
            contractId,
            transfer =>
            {
                transfer.State = state;
                transfer.ErrorCode = null;
            },
            required: true,
            cancellationToken);

    public Task MarkFailureAsync(
        Guid organizationId,
        Guid contractId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return UpdateAsync(
            organizationId,
            contractId,
            transfer =>
            {
                transfer.State = ArchiveTransferStates.Failed;
                transfer.ErrorCode = exception.GetType().Name;
            },
            required: false,
            cancellationToken);
    }

    private async Task UpdateAsync(
        Guid organizationId,
        Guid contractId,
        Action<ContractArchiveTransferEntity> update,
        bool required,
        CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        var transfer = await context.ContractArchiveTransfers.FindAsync([organizationId, contractId], cancellationToken);

        if (transfer is null)
        {
            if (required)
            {
                throw new InvalidOperationException("archiveTransferMissing");
            }

            return;
        }

        update(transfer);
        transfer.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);
    }
}
