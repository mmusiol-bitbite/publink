namespace Audit.Application.Queries;

public interface IContractTimelineSource
{
    Task<TimelinePage> ReadAsync(
        Guid contractId,
        int limit,
        string? cursor,
        TimelineFilter filter,
        CancellationToken cancellationToken);
}
