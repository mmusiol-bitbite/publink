using Audit.Application.Queries;
using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Queries;

public interface IContractReadSource
{
    ContractAuditDataSource DataSource { get; }

    string SearchSql { get; }

    string TimelineSql { get; }

    string SnapshotSql { get; }

    string TimestampSql { get; }

    string UnavailableMessage { get; }

    SqlConnection CreateConnection();
}
