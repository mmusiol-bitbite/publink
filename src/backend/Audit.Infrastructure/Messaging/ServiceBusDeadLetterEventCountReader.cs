using Audit.Application.Queries;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Messaging;

public sealed partial class ServiceBusDeadLetterEventCountReader(
    ServiceBusAdministrationClient administration,
    string queueName,
    ILogger<ServiceBusDeadLetterEventCountReader> logger)
    : IDeadLetterEventCountReader
{
    public async Task<long?> ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var properties = await administration.GetQueueRuntimePropertiesAsync(
                queueName,
                cancellationToken);
            return properties.Value.DeadLetterMessageCount;
        }
        catch (Exception exception) when (exception is RequestFailedException or ServiceBusException)
        {
            LogUnavailable(logger, queueName, exception);
            return null;
        }
    }

    [LoggerMessage(
        EventId = 4201,
        Level = LogLevel.Warning,
        Message = "Could not read the dead-letter count for queue {QueueName}.")]
    private static partial void LogUnavailable(ILogger logger, string queueName, Exception exception);
}
