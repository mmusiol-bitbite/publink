using Audit.Application.Legacy;

namespace Audit.Ingestion.Worker;

internal sealed class LegacySynchronizationCoordinator(
    IServiceScopeFactory scopeFactory,
    LegacyImportOptions options) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<ImportSweepResult> RunToCurrentAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var publishedCount = 0;
            ImportBatchResult batch;
            do
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var importer = scope.ServiceProvider.GetRequiredService<LegacyAuditImporter>();
                batch = await importer.ImportNextBatchAsync(cancellationToken);
                publishedCount += batch.PublishedCount;
            }
            while (batch.PublishedCount == options.BatchSize && !cancellationToken.IsCancellationRequested);

            await using var completionScope = scopeFactory.CreateAsyncScope();
            var completionImporter = completionScope.ServiceProvider
                .GetRequiredService<LegacyAuditImporter>();
            await completionImporter.MarkSweepCompletedAsync(cancellationToken);

            return new ImportSweepResult(publishedCount, batch.LastSourceEventId);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();
}

internal sealed record ImportSweepResult(int PublishedCount, long LastSourceEventId);
