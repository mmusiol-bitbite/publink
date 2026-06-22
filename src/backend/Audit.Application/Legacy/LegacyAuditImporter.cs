namespace Audit.Application.Legacy;

public sealed class LegacyAuditImporter(
    ILegacyAuditReader reader,
    IImportCheckpointStore checkpoints,
    IAuditEventPublisher publisher,
    LegacyAuditEventMapper mapper,
    TimeProvider timeProvider,
    LegacyImportOptions options)
{
    public async Task<ImportBatchResult> ImportNextBatchAsync(CancellationToken cancellationToken)
    {
        var checkpoint = await checkpoints.GetAsync(options.Source, cancellationToken);
        var records = await reader.ReadAfterAsync(checkpoint, options.BatchSize, cancellationToken);

        foreach (var record in records)
        {
            await publisher.PublishAsync(mapper.Map(record), cancellationToken);
            await checkpoints.SaveAsync(
                options.Source,
                record.SourceEventId,
                timeProvider.GetUtcNow(),
                cancellationToken);
        }

        return new ImportBatchResult(
            records.Count,
            records.Count > 0 ? records[records.Count - 1].SourceEventId : checkpoint);
    }

    public Task MarkSweepCompletedAsync(CancellationToken cancellationToken) =>
        checkpoints.MarkSynchronizedAsync(
            options.Source,
            timeProvider.GetUtcNow(),
            cancellationToken);
}

public sealed record ImportBatchResult(int PublishedCount, long LastSourceEventId);
