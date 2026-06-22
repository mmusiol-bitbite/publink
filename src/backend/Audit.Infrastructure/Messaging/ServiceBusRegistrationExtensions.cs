using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure.Messaging;

public static class ServiceBusRegistrationExtensions
{
    public static readonly TimeSpan RetryInitialInterval = TimeSpan.FromMilliseconds(200);
    public static readonly TimeSpan RetryMaxInterval = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan RetryIntervalDelta = TimeSpan.FromMilliseconds(200);

    public static IServiceCollection AddAuditServiceBusClients(
        this IServiceCollection services,
        string serviceBusConnection,
        string serviceBusAdministrationConnection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceBusConnection);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceBusAdministrationConnection);

        ServiceBusEmulatorCompatibility.ApplyIfRequired(serviceBusConnection);
        services.AddSingleton(_ => new ServiceBusClient(serviceBusConnection));
        services.AddSingleton(_ =>
            new ServiceBusAdministrationClient(serviceBusAdministrationConnection));
        return services;
    }

    public static void ConfigureAuditServiceBusHost(
        this IServiceBusBusFactoryConfigurator bus,
        IBusRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(context);

        var client = context.GetRequiredService<ServiceBusClient>();
        bus.Host(
            new Uri($"sb://{client.FullyQualifiedNamespace}"),
            client,
            context.GetRequiredService<ServiceBusAdministrationClient>());
    }

    public static void UseAuditConsumerFailurePolicy(
        this IServiceBusReceiveEndpointConfigurator endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        endpoint.UseMessageRetry(retry => retry.Exponential(
            3,
            RetryInitialInterval,
            RetryMaxInterval,
            RetryIntervalDelta));
        endpoint.ConfigureDeadLetterQueueDeadLetterTransport();
        endpoint.ConfigureDeadLetterQueueErrorTransport();
    }
}
