namespace Audit.Application.Legacy;

public interface IImportCheckpointStore
{
    Task<long> GetAsync(string source, CancellationToken cancellationToken);

    Task SaveAsync(
        string source,
        long sourceEventId,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);

    Task MarkSynchronizedAsync(
        string source,
        DateTimeOffset synchronizedAt,
        CancellationToken cancellationToken);
}
