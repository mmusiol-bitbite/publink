using Audit.Application.Legacy;
using Audit.Application.Queries;
using Audit.Infrastructure.Observability;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Core;
using Audit.Infrastructure.Persistence.Persisters;
using Audit.Infrastructure.Queries;
using Audit.Infrastructure.Queries.ReadSources;

namespace Audit.Query.Api.Bootstrap;

internal static class InfrastructureServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration,
        InfrastructureSettings settings)
    {
        services.AddAuditObservability(
            configuration,
            "audit-query-api",
            instrumentAspNetCore: true);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new SqlConnectionFactory(settings.ReadModelConnection));
        services.AddSingleton(new ArchiveSqlConnectionFactory(settings.ArchiveConnection));
        services.AddSingleton<ActiveContractReadSource>();
        services.AddSingleton<ArchivedContractReadSource>();

        services.AddScoped<ISynchronizationStatusReader, SynchronizationStatusReader>();
        services.AddScoped<ILegacySynchronizationRequestStore, EfLegacySynchronizationRequestStore>();

        services.AddAuditReadModelPersistence(settings.ReadModelConnection);
        services.AddAuditArchivePersistence(settings.ArchiveConnection);
        services.AddDatabaseStartupInitializer();
    }
}
