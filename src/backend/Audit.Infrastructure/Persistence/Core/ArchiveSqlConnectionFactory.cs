using Microsoft.Data.SqlClient;

namespace Audit.Infrastructure.Persistence.Core;

public sealed class ArchiveSqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}
