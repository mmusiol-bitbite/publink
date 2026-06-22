namespace Audit.Application.Queries;

public interface ISynchronizationStatusReader
{
    Task<SynchronizationStatus?> ReadAsync(
        string source,
        TimeSpan healthyStatusMaxAge,
        CancellationToken cancellationToken);
}
