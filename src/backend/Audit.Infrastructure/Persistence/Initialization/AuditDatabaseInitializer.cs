using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Core;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Initialization;

public sealed class AuditDatabaseInitializer(AuditDbContext dbContext) : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
