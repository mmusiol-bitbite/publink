using Audit.Application.Legacy;
using Audit.Contracts;
using MassTransit;

namespace Audit.Ingestion.Worker;

internal sealed partial class RequestLegacySynchronizationConsumer(
    LegacySynchronizationCoordinator coordinator,
    ILegacySynchronizationRequestStore requestStore,
    LegacyImportOptions options,
    ILogger<RequestLegacySynchronizationConsumer> logger)
    : IConsumer<RequestLegacySynchronizationV1>
{
    public async Task Consume(ConsumeContext<RequestLegacySynchronizationV1> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!string.Equals(context.Message.Source, options.Source, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("synchronizationSourceUnsupported");
        }

        var result = await coordinator.RunToCurrentAsync(context.CancellationToken);
        await requestStore.CompleteAsync(
            context.Message.CorrelationId,
            context.CancellationToken);
        LogCompleted(
            logger,
            context.Message.CorrelationId,
            result.PublishedCount,
            result.LastSourceEventId);
    }

    [LoggerMessage(
        EventId = 1101,
        Level = LogLevel.Information,
        Message = "Manual synchronization {CorrelationId} completed: {PublishedCount} events through source sequence {SourceSequence}")]
    private static partial void LogCompleted(
        ILogger logger,
        Guid correlationId,
        int publishedCount,
        long sourceSequence);
}
