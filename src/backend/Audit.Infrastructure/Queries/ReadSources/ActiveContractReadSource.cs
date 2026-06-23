using Audit.Application.Queries;
using Audit.Infrastructure.Persistence.Core;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Queries.ReadSources;

// Raw SQL is used because contract read queries involve LIKE searches across multiple columns
// and parameterised table names (active vs. archived views). See ContractReadSql for definitions.
public sealed class ActiveContractReadSource(SqlConnectionFactory connections) : IContractReadSource
{
    public ContractAuditDataSource DataSource => ContractAuditDataSource.Active;

    public string SearchSql => ContractReadSql.Search(
        "contract_search",
        "contract_search_aliases",
        """
          AND NOT EXISTS (
              SELECT 1
              FROM contract_archive_transfers transfer
              WHERE transfer.OrganizationId = contract_row.OrganizationId
                AND transfer.ContractId = contract_row.ContractId
                AND transfer.State IN ('ReactivationPending', 'ReactivatedCopied')
          )
        """);

    public string TimelineSql => ContractReadSql.Timeline("contract_timeline_items");

    public string SnapshotSql => ContractReadSql.Snapshot("contract_timeline_items");

    public string TimestampSql => "SELECT MAX(UpdatedAt) FROM import_checkpoints;";

    public string UnavailableMessage => "The active contract database is unavailable.";

    public SqlConnection CreateConnection() => connections.Create();
}
