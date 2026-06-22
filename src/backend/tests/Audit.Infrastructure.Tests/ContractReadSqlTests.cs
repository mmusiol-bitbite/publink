using Audit.Infrastructure.Queries.ReadSources;

namespace Audit.Infrastructure.Tests;

public sealed class ContractReadSqlTests
{
    [Fact]
    public void SearchIncludesContractIdPredicate()
    {
        var activeSql = new ActiveContractReadSource(null!).SearchSql;
        var archivedSql = new ArchivedContractReadSource(null!).SearchSql;

        activeSql.Should().Contain("CONVERT(varchar(36), contract_row.ContractId) LIKE @Pattern");
        archivedSql.Should().Contain("CONVERT(varchar(36), contract_row.ContractId) LIKE @Pattern");
    }

    [Fact]
    public void ActiveSearchDoesNotFilterByRequestOrganizationId()
    {
        var sql = new ActiveContractReadSource(null!).SearchSql;

        sql.Should().NotContain("contract_row.OrganizationId = @OrganizationId");
        sql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
    }

    [Fact]
    public void ArchivedSearchDoesNotFilterByRequestOrganizationId()
    {
        var sql = new ArchivedContractReadSource(null!).SearchSql;

        sql.Should().NotContain("contract_row.OrganizationId = @OrganizationId");
        sql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
    }

    [Fact]
    public void TimelineDetailsDoNotFilterByRequestOrganizationId()
    {
        var activeTimelineSql = new ActiveContractReadSource(null!).TimelineSql;
        var archivedTimelineSql = new ArchivedContractReadSource(null!).TimelineSql;
        var activeSnapshotSql = new ActiveContractReadSource(null!).SnapshotSql;
        var archivedSnapshotSql = new ArchivedContractReadSource(null!).SnapshotSql;
        var archivedTimestampSql = new ArchivedContractReadSource(null!).TimestampSql;

        activeTimelineSql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
        archivedTimelineSql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
        activeSnapshotSql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
        archivedSnapshotSql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
        archivedTimestampSql.Should().NotContain("WHERE OrganizationId = @OrganizationId");
    }
}
