namespace Audit.Application.Legacy;

public interface ILegacySynchronizationRequestStore
{
    Task<LegacySynchronizationRequestLease> AcquireAsync(
        string source,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken);

    Task ReleaseAsync(Guid correlationId, CancellationToken cancellationToken);

    Task CompleteAsync(Guid correlationId, CancellationToken cancellationToken);
}
