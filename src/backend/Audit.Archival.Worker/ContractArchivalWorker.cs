using Audit.Application.Archiving;

namespace Audit.Archival.Worker;

internal sealed partial class ContractArchivalWorker(
    IServiceScopeFactory scopeFactory,
    ContractArchivalOptions options,
    ILogger<ContractArchivalWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = 0;
                while (processed < options.MaximumContractsPerRun && !stoppingToken.IsCancellationRequested)
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var lifecycle = scope.ServiceProvider.GetRequiredService<IContractArchiveLifecycle>();
                    var result = await lifecycle.RunOnceAsync(stoppingToken);
                    if (!result.Processed)
                    {
                        break;
                    }

                    processed++;
                    LogProcessed(
                        logger,
                        result.Action,
                        result.OrganizationId,
                        result.ContractId,
                        result.EventCount);
                }

                await Task.Delay(options.RunInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogFailed(logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Contract archival action {Action} completed for {OrganizationId}/{ContractId}: {EventCount} events")]
    private static partial void LogProcessed(
        ILogger logger,
        string? action,
        Guid? organizationId,
        Guid? contractId,
        int eventCount);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Error,
        Message = "Contract archival iteration failed; retrying")]
    private static partial void LogFailed(ILogger logger, Exception exception);

}
