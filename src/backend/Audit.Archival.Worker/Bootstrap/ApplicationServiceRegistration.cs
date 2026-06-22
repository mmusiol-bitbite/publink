namespace Audit.Archival.Worker.Bootstrap;

internal static class ApplicationServiceRegistration
{
    public static void Register(IServiceCollection services) =>
        services.AddHostedService<ContractArchivalWorker>();
}
