using Audit.Contracts;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence.Contexts;
using MassTransit;

namespace Audit.Processing.Worker.Bootstrap;

internal static class MessagingServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        ProcessingSettings settings)
    {
        services.AddAuditServiceBusClients(
            settings.ServiceBusConnection,
            settings.ServiceBusAdministrationConnection);

        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.AddConsumer<AuditEntryImportedConsumer>();
            configurator.AddEntityFrameworkOutbox<AuditDbContext>(outbox =>
            {
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });
            configurator.UsingAzureServiceBus((context, bus) =>
            {
                bus.ConfigureAuditServiceBusHost(context);
                bus.Message<AuditEntryImportedV1>(message =>
                    message.SetEntityName("audit-entry-imported-v1"));
                bus.Publish<AuditEntryImportedV1>(entity =>
                    entity.DefaultMessageTimeToLive = TimeSpan.FromHours(1));
                bus.ReceiveEndpoint("audit-projection", endpoint =>
                {
                    endpoint.DefaultMessageTimeToLive = TimeSpan.FromHours(1);
                    endpoint.MaxDeliveryCount = 5;
                    endpoint.UseAuditConsumerFailurePolicy();
                    endpoint.UseEntityFrameworkOutbox<AuditDbContext>(context);
                    endpoint.ConfigureConsumer<AuditEntryImportedConsumer>(context);
                });
            });
        });
    }

}
