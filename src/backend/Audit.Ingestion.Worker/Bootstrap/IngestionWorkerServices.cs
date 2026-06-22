namespace Audit.Ingestion.Worker.Bootstrap;

internal static class IngestionWorkerServices
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = IngestionSettings.From(configuration);
        ApplicationServiceRegistration.Register(services);
        InfrastructureServiceRegistration.Register(services, configuration, settings);
        MessagingServiceRegistration.Register(services, settings);
    }
}
