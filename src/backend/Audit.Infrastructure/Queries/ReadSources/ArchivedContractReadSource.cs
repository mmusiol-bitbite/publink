using Audit.Application.Queries;
using Audit.Infrastructure.Persistence.Core;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Queries.ReadSources;

// Raw SQL is used because contract read queries involve LIKE searches across multiple columns
// and parameterised table names (active vs. archived views). See ContractReadSql for definitions.
public sealed class ArchivedContractReadSource(ArchiveSqlConnectionFactory connections): IContractReadSource
{
    public ContractAuditDataSource DataSource => ContractAuditDataSource.Archive;

    public string SearchSql => ContractReadSql.Search(
        "archived_contracts",
        "archived_contract_aliases");

    public string TimelineSql => ContractReadSql.Timeline("archived_timeline_items");

    public string SnapshotSql => ContractReadSql.Snapshot("archived_timeline_items");

    public string TimestampSql => """
        SELECT ArchivedAt
        FROM archived_contracts
        WHERE ContractId = @ContractId;
        """;

    public string UnavailableMessage => "The contract archive database is unavailable.";

    public SqlConnection CreateConnection() => connections.Create();
}
