using Audit.Application.Queries;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Queries;

public static class ContractTimelineQueryExecutor
{
    public static async Task<TimelinePage> ExecuteAsync(
        IContractReadSource source,
        Guid contractId,
        int limit,
        string? cursor,
        TimelineFilter filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        var cursorValue = TimelineQuerySupport.DecodeCursor(cursor);
        try
        {
            await using var connection = source.CreateConnection();
            var snapshot = cursorValue?.SnapshotSequence
                ?? await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
                    source.SnapshotSql,
                    new { ContractId = contractId },
                    cancellationToken: cancellationToken))
                ?? 0;

            var actorPattern = string.IsNullOrWhiteSpace(filter.Actor)
                ? null
                : SqlLikePattern.Contains(filter.Actor.Trim());
            var parameters = new
            {
                ContractId = contractId,
                Snapshot = snapshot,
                BeforeSequence = cursorValue?.BeforeSequence,
                filter.From,
                filter.To,
                ActorPattern = actorPattern,
                filter.ChangeType,
                filter.EntityType,
                Take = limit + 1
            };
            var rows = (await connection.QueryAsync<TimelineRow>(new CommandDefinition(
                source.TimelineSql,
                parameters,
                cancellationToken: cancellationToken))).AsList();

            var hasMore = rows.Count > limit;
            var pageRows = rows.Take(limit).ToArray();
            var timestamp = await connection.ExecuteScalarAsync<DateTimeOffset?>(
                new CommandDefinition(
                    source.TimestampSql,
                    new { ContractId = contractId },
                    cancellationToken: cancellationToken));

            return new TimelinePage(
                contractId,
                snapshot,
                timestamp,
                pageRows.Select(TimelineQuerySupport.Map).ToArray(),
                hasMore && pageRows.Length > 0
                    ? TimelineQuerySupport.EncodeCursor(snapshot, pageRows[^1].SourceSequence)
                    : null);
        }
        catch (Exception exception) when (exception is SqlException or TimeoutException)
        {
            throw new ContractStoreUnavailableException(source.UnavailableMessage, exception);
        }
    }
}
