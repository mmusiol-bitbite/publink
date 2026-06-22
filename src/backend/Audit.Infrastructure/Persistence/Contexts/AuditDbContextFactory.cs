using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Audit.Infrastructure.Persistence.Contexts;

public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUDIT_READ_MODEL_CONNECTION")
            ?? throw new InvalidOperationException(
                "AUDIT_READ_MODEL_CONNECTION is required for design-time EF operations.");
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AuditDbContext(options);
    }
}
