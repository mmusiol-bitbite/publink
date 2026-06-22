using Audit.Application.Legacy;

namespace Audit.Ingestion.Worker.Bootstrap;

internal static class ApplicationServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<LegacyAuditEventMapper>();
        services.AddScoped<LegacyAuditImporter>();
        services.AddSingleton<LegacySynchronizationCoordinator>();
        services.AddHostedService<LegacyImportWorker>();
    }
}
