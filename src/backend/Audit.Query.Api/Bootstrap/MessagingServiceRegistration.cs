using Audit.Application.Legacy;
using Audit.Application.Queries;
using Audit.Infrastructure.Messaging;
using Audit.Query.Api.Configuration;
using Azure.Messaging.ServiceBus.Administration;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Audit.Query.Api.Bootstrap;

internal static class MessagingServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        InfrastructureSettings settings)
    {
        services.AddAuditServiceBusClients(
            settings.ServiceBusConnection,
            settings.ServiceBusAdministrationConnection);
        services.AddSingleton<IDeadLetterEventCountReader>(provider =>
            new ServiceBusDeadLetterEventCountReader(
                provider.GetRequiredService<ServiceBusAdministrationClient>(),
                provider.GetRequiredService<IOptions<SynchronizationOptions>>()
                    .Value.DeadLetterQueue,
                provider.GetRequiredService<ILogger<ServiceBusDeadLetterEventCountReader>>()));
        services.AddScoped<ILegacySynchronizationRequester>(provider =>
            new MassTransitLegacySynchronizationRequester(
                provider.GetRequiredService<ISendEndpointProvider>(),
                provider.GetRequiredService<IOptions<SynchronizationOptions>>().Value.LegacySynchronizationQueueName));
        services.AddMassTransit(configurator =>
            configurator.UsingAzureServiceBus((context, bus) =>
                bus.ConfigureAuditServiceBusHost(context)));
    }
}
