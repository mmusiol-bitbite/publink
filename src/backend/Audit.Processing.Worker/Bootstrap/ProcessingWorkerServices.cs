namespace Audit.Processing.Worker.Bootstrap;

internal static class ProcessingWorkerServices
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = ProcessingSettings.From(configuration);
        InfrastructureServiceRegistration.Register(services, configuration, settings);
        MessagingServiceRegistration.Register(services, settings);
    }
}
