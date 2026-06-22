using Audit.Application.Legacy;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Legacy;

public sealed class SqlLegacyAuditReader(string connectionString) : ILegacyAuditReader
{
    public async Task<IReadOnlyList<LegacyAuditRecord>> ReadAfterAsync(
        long sourceEventId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (@BatchSize)
                Id AS SourceEventId,
                OrganizationId,
                UserId,
                UserEmail,
                Type,
                EntityType,
                CreatedDate,
                OldValues,
                NewValues,
                AffectedColumns,
                PrimaryKey,
                EntityId,
                ParentId,
                CorrelationId
            FROM dbo.AuditLog
            WHERE Id > @SourceEventId
            ORDER BY Id;
            """;

        await using var connection = new SqlConnection(connectionString);
        var rows = await connection.QueryAsync<LegacyAuditRecord>(
            new CommandDefinition(
                sql,
                new { SourceEventId = sourceEventId, BatchSize = batchSize },
                commandTimeout: 15,
                cancellationToken: cancellationToken));

        return rows.AsList();
    }
}

