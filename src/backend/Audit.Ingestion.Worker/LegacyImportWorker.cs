using Audit.Application.Legacy;

namespace Audit.Ingestion.Worker;

internal sealed partial class LegacyImportWorker(
    LegacySynchronizationCoordinator coordinator,
    LegacyImportOptions options,
    ILogger<LegacyImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await coordinator.RunToCurrentAsync(stoppingToken);
                LogPublished(
                    logger,
                    result.PublishedCount,
                    result.LastSourceEventId);

                await Task.Delay(options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogImportFailed(logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Scheduled synchronization published {PublishedCount} legacy audit events through source sequence {SourceSequence}")]
    private static partial void LogPublished(
        ILogger logger,
        int publishedCount,
        long sourceSequence);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Legacy audit import failed; retrying after a bounded delay")]
    private static partial void LogImportFailed(ILogger logger, Exception exception);

}
