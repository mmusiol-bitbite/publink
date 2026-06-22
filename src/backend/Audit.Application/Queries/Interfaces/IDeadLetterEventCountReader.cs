namespace Audit.Application.Queries;

public interface IDeadLetterEventCountReader
{
    Task<long?> ReadAsync(CancellationToken cancellationToken);
}
