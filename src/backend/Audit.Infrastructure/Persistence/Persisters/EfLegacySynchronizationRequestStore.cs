using System.Data;
using Audit.Application.Legacy;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Persisters;

public sealed class EfLegacySynchronizationRequestStore(
    AuditDbContext dbContext,
    TimeProvider timeProvider) : ILegacySynchronizationRequestStore
{
    public async Task<LegacySynchronizationRequestLease> AcquireAsync(
        string source,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        // UPDLOCK and HOLDLOCK table hints are required to acquire a pessimistic row-level lock before
        // reading the request, preventing concurrent workers from processing the same source simultaneously.
        // EF Core does not support specifying table hints in standard LINQ queries.
        var existing = await dbContext.LegacySynchronizationRequests
            .FromSqlInterpolated(
                $"SELECT * FROM legacy_synchronization_requests WITH (UPDLOCK, HOLDLOCK) WHERE Source = {source}")
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is not null && existing.CompletedAt is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return new LegacySynchronizationRequestLease(
                existing.CorrelationId,
                existing.Source,
                existing.RequestedAt,
                false);
        }

        var correlationId = Guid.NewGuid();
        if (existing is null)
        {
            dbContext.LegacySynchronizationRequests.Add(new LegacySynchronizationRequestEntity
            {
                Source = source,
                CorrelationId = correlationId,
                RequestedAt = requestedAt
            });
        }
        else
        {
            existing.CorrelationId = correlationId;
            existing.RequestedAt = requestedAt;
            existing.CompletedAt = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new LegacySynchronizationRequestLease(correlationId, source, requestedAt, true);
    }

    public async Task ReleaseAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        var request = await dbContext.LegacySynchronizationRequests
            .SingleOrDefaultAsync(item => item.CorrelationId == correlationId, cancellationToken);
        if (request is null || request.CompletedAt is not null)
        {
            return;
        }

        dbContext.LegacySynchronizationRequests.Remove(request);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        var request = await dbContext.LegacySynchronizationRequests
            .SingleOrDefaultAsync(item => item.CorrelationId == correlationId, cancellationToken);
        if (request is null || request.CompletedAt is not null)
        {
            return;
        }

        request.CompletedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
