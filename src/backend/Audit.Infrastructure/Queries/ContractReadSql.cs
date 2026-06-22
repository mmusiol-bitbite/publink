namespace Audit.Infrastructure.Queries;

// Raw SQL is used because the queries combine full-text LIKE searches, subquery-based ranking,
// and runtime-chosen table names (active vs. archived views). These cannot be expressed with
// EF Core LINQ in a way that generates an efficient single-statement query.
internal static class ContractReadSql
{
    public static string Search(
        string contractTable,
        string aliasTable,
        string additionalPredicate = "") => $$"""
        SELECT TOP (@Limit)
            contract_row.ContractId,
            contract_row.Number,
            contract_row.InternalNumber,
            contract_row.Subject,
            contract_row.ContractorName,
            contract_row.LastActivityAt,
            CAST(CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM {{aliasTable}} exact_alias
                    WHERE exact_alias.OrganizationId = contract_row.OrganizationId
                      AND exact_alias.ContractId = contract_row.ContractId
                      AND exact_alias.Value = @ExactQuery)
                  AND COALESCE(contract_row.Number, '') <> @ExactQuery
                  AND COALESCE(contract_row.InternalNumber, '') <> @ExactQuery
                  AND COALESCE(contract_row.Subject, '') <> @ExactQuery
                  AND COALESCE(contract_row.ContractorName, '') <> @ExactQuery
                THEN 1
                ELSE 0
                        END AS bit) AS MatchedHistoricalValue
                FROM {{contractTable}} contract_row
                WHERE 1 = 1
                {{additionalPredicate}}
                    AND (
              COALESCE(contract_row.Number, '') LIKE @Pattern ESCAPE '\'
              OR COALESCE(contract_row.InternalNumber, '') LIKE @Pattern ESCAPE '\'
              OR COALESCE(contract_row.Subject, '') LIKE @Pattern ESCAPE '\'
              OR COALESCE(contract_row.ContractorName, '') LIKE @Pattern ESCAPE '\'
                          OR CONVERT(varchar(36), contract_row.ContractId) LIKE @Pattern ESCAPE '\'
              OR EXISTS (
                  SELECT 1
                  FROM {{aliasTable}} alias
                  WHERE alias.OrganizationId = contract_row.OrganizationId
                    AND alias.ContractId = contract_row.ContractId
                    AND alias.Value LIKE @Pattern ESCAPE '\'
              )
          )
        ORDER BY contract_row.Number, contract_row.ContractId;
        """;

    public static string Timeline(string timelineTable) => $$"""
        SELECT TOP (@Take)
            EventId,
            SourceSequence,
            OccurredAt,
            CorrelationId,
            ChangeKind,
            ChangeTypeCode AS ChangeKindCode,
            EntityKind,
            EntityTypeCode AS EntityKindCode,
            Actor,
            ChangesJson,
            DataQualityIssuesJson
        FROM {{timelineTable}}
                WHERE ContractId = @ContractId
          AND SourceSequence <= @Snapshot
          AND (@BeforeSequence IS NULL OR SourceSequence < @BeforeSequence)
          AND (@From IS NULL OR OccurredAt >= @From)
          AND (@To IS NULL OR OccurredAt <= @To)
          AND (@ActorPattern IS NULL OR Actor LIKE @ActorPattern ESCAPE '\')
          AND (@ChangeType IS NULL OR ChangeTypeCode = @ChangeType)
          AND (@EntityType IS NULL OR EntityTypeCode = @EntityType)
        ORDER BY SourceSequence DESC;
        """;

    public static string Snapshot(string timelineTable) => $$"""
        SELECT MAX(SourceSequence)
        FROM {{timelineTable}}
        WHERE ContractId = @ContractId;
        """;
}
