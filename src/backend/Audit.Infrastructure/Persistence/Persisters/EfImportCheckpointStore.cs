using Audit.Application.Legacy;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Persisters;

public sealed class EfImportCheckpointStore(AuditDbContext dbContext)
    : IImportCheckpointStore
{
    public async Task<long> GetAsync(string source, CancellationToken cancellationToken) =>
        await dbContext.ImportCheckpoints
            .Where(item => item.Source == source)
            .Select(item => (long?)item.LastSourceEventId)
            .SingleOrDefaultAsync(cancellationToken)
        ?? 0;

    public async Task SaveAsync(
        string source,
        long sourceEventId,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var checkpoint = await dbContext.ImportCheckpoints.FindAsync([source], cancellationToken);
        if (checkpoint is null)
        {
            dbContext.ImportCheckpoints.Add(new ImportCheckpointEntity
            {
                Source = source,
                LastSourceEventId = sourceEventId,
                UpdatedAt = updatedAt
            });
        }
        else if (checkpoint.LastSourceEventId < sourceEventId)
        {
            checkpoint.LastSourceEventId = sourceEventId;
            checkpoint.UpdatedAt = updatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkSynchronizedAsync(
        string source,
        DateTimeOffset synchronizedAt,
        CancellationToken cancellationToken)
    {
        var checkpoint = await dbContext.ImportCheckpoints.FindAsync([source], cancellationToken);
        if (checkpoint is null)
        {
            dbContext.ImportCheckpoints.Add(new ImportCheckpointEntity
            {
                Source = source,
                LastSourceEventId = 0,
                UpdatedAt = synchronizedAt
            });
        }
        else
        {
            checkpoint.UpdatedAt = synchronizedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
