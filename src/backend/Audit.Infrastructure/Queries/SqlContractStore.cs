using Audit.Application.Queries;

namespace Audit.Infrastructure.Queries;

public sealed class SqlContractStore(IContractReadSource source) : IContractStore
{
    public ContractAuditDataSource DataSource => source.DataSource;

    public Task<IReadOnlyList<ContractSearchResult>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken) =>
        ContractSearchQueryExecutor.ExecuteAsync(
            source,
            query,
            limit,
            cancellationToken);

    public Task<TimelinePage> ReadAsync(
        Guid contractId,
        int limit,
        string? cursor,
        TimelineFilter filter,
        CancellationToken cancellationToken) =>
        ContractTimelineQueryExecutor.ExecuteAsync(
            source,
            contractId,
            limit,
            cursor,
            filter,
            cancellationToken);
}
