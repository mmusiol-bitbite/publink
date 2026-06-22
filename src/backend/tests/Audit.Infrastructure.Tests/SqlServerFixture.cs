using Audit.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Testcontainers.MsSql;

namespace Audit.Infrastructure.Tests;

[SuppressMessage("Performance", "CA1515", Justification = "xUnit ICollectionFixture requires public types.")]
public sealed class SqlServerFixture : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    private readonly MsSqlContainer container = new MsSqlBuilder(SqlServerImage).Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        ConnectionString = container.GetConnectionString();

        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlServer(ConnectionString)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var context = new AuditDbContext(options);
        await context.Database.MigrateAsync();

        var archiveOptions = new DbContextOptionsBuilder<ArchiveDbContext>()
            .UseSqlServer(ConnectionString)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var archiveContext = new ArchiveDbContext(archiveOptions);
        await archiveContext.Database.MigrateAsync();
    }

    public AuditDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new AuditDbContext(options);
    }

    public ArchiveDbContext CreateArchiveContext()
    {
        var options = new DbContextOptionsBuilder<ArchiveDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ArchiveDbContext(options);
    }

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
[SuppressMessage("Performance", "CA1515", Justification = "xUnit ICollectionFixture requires public types.")]
public sealed class SqlServerFixtureGroup : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
