namespace Audit.Archival.Worker.Bootstrap;

internal static class ArchivalWorkerServices
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = ArchivalSettings.From(configuration);
        ApplicationServiceRegistration.Register(services);
        InfrastructureServiceRegistration.Register(services, configuration, settings);
    }
}
