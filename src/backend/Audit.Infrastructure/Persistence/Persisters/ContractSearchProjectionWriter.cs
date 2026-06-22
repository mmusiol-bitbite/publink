using Audit.Contracts;
using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Persisters;

public sealed class ContractSearchProjectionWriter(AuditDbContext dbContext)
{
    private static readonly string[] SearchFields =
    [
        "Number",
        "InternalNumber",
        "Subject",
        "ContractorName"
    ];

    public async Task UpdateContractAsync(
        Guid organizationId,
        Guid contractId,
        AuditEntryImportedV1 message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var values = (message.After ?? message.Before).ReadFields();
        var number = values.ReadFieldValue("Number");
        var internalNumber = values.ReadFieldValue("InternalNumber");
        var subject = values.ReadFieldValue("Subject");
        var contractorName = values.ReadFieldValue("ContractorName");

        // MERGE with HOLDLOCK is used to perform an atomic upsert on contract_search. EF Core has no
        // built-in MERGE support, so raw SQL avoids a separate SELECT + INSERT/UPDATE round-trip.
        var currentProjectionChanged = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            MERGE contract_search WITH (HOLDLOCK) AS target
            USING (SELECT {organizationId} AS OrganizationId, {contractId} AS ContractId) AS source
               ON target.OrganizationId = source.OrganizationId
              AND target.ContractId = source.ContractId
            WHEN MATCHED AND target.LastSourceSequence < {message.SourceSequence} THEN
                UPDATE SET
                    Number = {number}, InternalNumber = {internalNumber}, Subject = {subject},
                    ContractorName = {contractorName}, LastSourceSequence = {message.SourceSequence}
            WHEN NOT MATCHED THEN
                INSERT (OrganizationId, ContractId, Number, InternalNumber, Subject,
                        ContractorName, LastSourceSequence, LastActivityAt)
                VALUES ({organizationId}, {contractId}, {number}, {internalNumber}, {subject},
                        {contractorName}, {message.SourceSequence}, {message.OccurredAt});
            """,
            cancellationToken);

        if (currentProjectionChanged > 0)
        {
            // Raw SQL updates all aliases for the contract in one statement. Using EF Core would require
            // loading every alias row first and issuing one UPDATE per entity instead of a single batch.
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE contract_search_aliases SET IsCurrent = 0
                WHERE OrganizationId = {organizationId} AND ContractId = {contractId};
                """,
                cancellationToken);
        }

        foreach (var field in SearchFields)
        {
            foreach (var value in ReadSearchValues(message, field))
            {
                var isCurrent = currentProjectionChanged > 0 && string.Equals(
                    value,
                    values.ReadFieldValue(field),
                    StringComparison.OrdinalIgnoreCase);
                await UpsertAliasAsync(
                    organizationId,
                    contractId,
                    field,
                    value,
                    isCurrent,
                    cancellationToken);
            }
        }
    }

    public Task<int> TrackActivityAsync(
        Guid organizationId,
        Guid contractId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken) =>
        // MERGE with HOLDLOCK performs an atomic upsert on contract_search and, in the same round-trip,
        // conditionally reactivates any archived contract transfer. EF Core has no built-in MERGE support.
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            MERGE contract_search WITH (HOLDLOCK) AS target
            USING (SELECT {organizationId} AS OrganizationId, {contractId} AS ContractId) AS source
               ON target.OrganizationId = source.OrganizationId
              AND target.ContractId = source.ContractId
            WHEN MATCHED AND target.LastActivityAt < {occurredAt} THEN
                UPDATE SET LastActivityAt = {occurredAt}
            WHEN NOT MATCHED THEN
                INSERT (OrganizationId, ContractId, LastSourceSequence, LastActivityAt)
                VALUES ({organizationId}, {contractId}, 0, {occurredAt});

            UPDATE contract_archive_transfers
                        SET State = {ArchiveTransferStates.ReactivationPending}, UpdatedAt = SYSUTCDATETIME()
            WHERE OrganizationId = {organizationId}
              AND ContractId = {contractId}
                            AND State = {ArchiveTransferStates.Archived};
            """,
            cancellationToken);

    private Task<int> UpsertAliasAsync(
        Guid organizationId,
        Guid contractId,
        string field,
        string value,
        bool isCurrent,
        CancellationToken cancellationToken) =>
        // Raw SQL performs an alias upsert (update IsCurrent on an existing row, or insert a new one).
        // EF Core AddOrUpdate would require reading the existing row first; this avoids the extra round-trip.
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            IF EXISTS (
                SELECT 1 FROM contract_search_aliases WITH (UPDLOCK, HOLDLOCK)
                WHERE OrganizationId = {organizationId} AND ContractId = {contractId}
                  AND Field = {field} AND Value = {value})
            BEGIN
                UPDATE contract_search_aliases
                SET IsCurrent = CASE WHEN {isCurrent} = 1 THEN 1 ELSE IsCurrent END
                WHERE OrganizationId = {organizationId} AND ContractId = {contractId}
                  AND Field = {field} AND Value = {value};
            END
            ELSE
            BEGIN
                INSERT INTO contract_search_aliases
                    (OrganizationId, ContractId, Field, Value, IsCurrent)
                VALUES ({organizationId}, {contractId}, {field}, {value}, {isCurrent});
            END;
            """,
            cancellationToken);

    private static IEnumerable<string> ReadSearchValues(AuditEntryImportedV1 message, string field)
    {
        var before = message.Before.ReadFields().ReadFieldValue(field);
        var after = message.After.ReadFields().ReadFieldValue(field);
        return new[] { before, after }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
