using Audit.Application.Queries;
using Audit.Infrastructure.Persistence.Core;
using Dapper;

namespace Audit.Infrastructure.Queries;

public sealed class SynchronizationStatusReader(
    SqlConnectionFactory connections,
    IDeadLetterEventCountReader deadLetters,
    TimeProvider timeProvider) : ISynchronizationStatusReader
{
    public async Task<SynchronizationStatus?> ReadAsync(
        string source,
        TimeSpan healthyStatusMaxAge,
        CancellationToken cancellationToken)
    {
        const string statusSql = """
            SELECT
                c.Source,
                c.LastSourceEventId,
                c.UpdatedAt AS LastSynchronizedAt,
                r.CorrelationId,
                r.RequestedAt
            FROM import_checkpoints AS c
            LEFT JOIN legacy_synchronization_requests AS r
                ON r.Source = c.Source AND r.CompletedAt IS NULL
            WHERE c.Source = @Source;
            """;

        await using var connection = connections.Create();
        var row = await connection.QuerySingleOrDefaultAsync<StatusRow>(
            new CommandDefinition(
                statusSql,
                new { Source = source },
                cancellationToken: cancellationToken));

        var deadLetterEventCount = await deadLetters.ReadAsync(cancellationToken);

        return row is null
            ? new SynchronizationStatus(source, 0, null, "initializing", deadLetterEventCount, null, null)
            : new SynchronizationStatus(
                row.Source,
                row.LastSourceEventId,
                row.LastSynchronizedAt,
                deadLetterEventCount > 0 ||
                timeProvider.GetUtcNow() - row.LastSynchronizedAt > healthyStatusMaxAge
                    ? "degraded"
                    : "healthy",
                deadLetterEventCount,
                row.CorrelationId,
                row.RequestedAt);
    }

    private sealed record StatusRow(
        string Source,
        long LastSourceEventId,
        DateTimeOffset LastSynchronizedAt,
        Guid? CorrelationId,
        DateTimeOffset? RequestedAt);
}
