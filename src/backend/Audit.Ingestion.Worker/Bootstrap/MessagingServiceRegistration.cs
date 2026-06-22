using Audit.Application.Legacy;
using Audit.Contracts;
using Audit.Infrastructure.Messaging;
using MassTransit;

namespace Audit.Ingestion.Worker.Bootstrap;

internal static class MessagingServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IngestionSettings settings)
    {
        services.AddAuditServiceBusClients(
            settings.ServiceBusConnection,
            settings.ServiceBusAdministrationConnection);
        services.AddScoped<IAuditEventPublisher, MassTransitAuditEventPublisher>();

        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.AddConsumer<RequestLegacySynchronizationConsumer>();
            configurator.UsingAzureServiceBus((context, bus) =>
            {
                bus.ConfigureAuditServiceBusHost(context);
                bus.Message<AuditEntryImportedV1>(message =>
                    message.SetEntityName("audit-entry-imported-v1"));
                bus.Publish<AuditEntryImportedV1>(entity =>
                    entity.DefaultMessageTimeToLive = TimeSpan.FromHours(1));
                bus.ReceiveEndpoint("legacy-synchronization", endpoint =>
                {
                    endpoint.ConcurrentMessageLimit = 1;
                    endpoint.MaxDeliveryCount = 5;
                    endpoint.UseAuditConsumerFailurePolicy();
                    endpoint.ConfigureConsumer<RequestLegacySynchronizationConsumer>(context);
                });
            });
        });
    }

}
