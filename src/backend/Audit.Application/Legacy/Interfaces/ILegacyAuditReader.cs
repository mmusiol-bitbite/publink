namespace Audit.Application.Legacy;

public interface ILegacyAuditReader
{
    Task<IReadOnlyList<LegacyAuditRecord>> ReadAfterAsync(
        long sourceEventId,
        int batchSize,
        CancellationToken cancellationToken);
}
