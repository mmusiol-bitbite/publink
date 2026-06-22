using Audit.Application.Legacy;
using Audit.Infrastructure.Legacy;
using Audit.Infrastructure.Observability;
using Audit.Infrastructure.Persistence.Core;
using Audit.Infrastructure.Persistence.Persisters;

namespace Audit.Ingestion.Worker.Bootstrap;

internal static class InfrastructureServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration,
        IngestionSettings settings)
    {
        services.AddAuditObservability(configuration, "audit-ingestion-worker");
        services.AddSingleton(settings.ImportOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddAuditReadModelPersistence(settings.ReadModelConnection);
        services.AddDatabaseStartupInitializer();
        services.AddScoped<IImportCheckpointStore, EfImportCheckpointStore>();
        services.AddScoped<ILegacySynchronizationRequestStore, EfLegacySynchronizationRequestStore>();
        services.AddSingleton<ILegacyAuditReader>(new SqlLegacyAuditReader(settings.LegacySourceConnection));
    }
}
