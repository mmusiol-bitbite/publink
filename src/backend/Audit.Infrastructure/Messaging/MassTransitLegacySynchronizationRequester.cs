using Audit.Application.Legacy;
using Audit.Contracts;
using Azure.Messaging.ServiceBus;
using MassTransit;

namespace Audit.Infrastructure.Messaging;

public sealed class MassTransitLegacySynchronizationRequester(
    ISendEndpointProvider sendEndpointProvider,
    string queueName) : ILegacySynchronizationRequester
{
    private readonly Uri _destination = new($"queue:{queueName}");

    public async Task RequestAsync(RequestLegacySynchronizationV1 command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(_destination);
            await endpoint.Send(command, cancellationToken);
        }
        catch (Exception exception) when (exception is ServiceBusException or MassTransitException or TimeoutException)
        {
            throw new LegacySynchronizationUnavailableException("The synchronization command could not be delivered.", exception);
        }
    }
}
