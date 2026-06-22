using Audit.Application.Queries;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Queries;

public static class ContractSearchQueryExecutor
{
    public static async Task<IReadOnlyList<ContractSearchResult>> ExecuteAsync(
        IContractReadSource source,
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);

        var normalizedQuery = query.Trim();
        try
        {
            await using var connection = source.CreateConnection();
            var rows = await connection.QueryAsync<ContractSearchResult>(new CommandDefinition(
                source.SearchSql,
                new
                {
                    Pattern = SqlLikePattern.Contains(normalizedQuery),
                    ExactQuery = normalizedQuery,
                    Limit = limit
                },
                cancellationToken: cancellationToken));
            return rows.AsList();
        }
        catch (Exception exception) when (exception is SqlException or TimeoutException)
        {
            throw new ContractStoreUnavailableException(source.UnavailableMessage, exception);
        }
    }
}
