using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Audit.Infrastructure.Persistence.Contexts;

public sealed class ArchiveDbContextFactory : IDesignTimeDbContextFactory<ArchiveDbContext>
{
    public ArchiveDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUDIT_ARCHIVE_CONNECTION")
            ?? throw new InvalidOperationException(
                "AUDIT_ARCHIVE_CONNECTION is required for design-time EF operations.");
        var options = new DbContextOptionsBuilder<ArchiveDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ArchiveDbContext(options);
    }
}
