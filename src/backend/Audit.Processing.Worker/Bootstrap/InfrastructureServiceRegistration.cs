using Audit.Application.Persistence;
using Audit.Infrastructure.Observability;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Core;
using Audit.Infrastructure.Persistence.Persisters;

namespace Audit.Processing.Worker.Bootstrap;

internal static class InfrastructureServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration,
        ProcessingSettings settings)
    {
        services.AddAuditObservability(configuration, "audit-processing-worker");
        services.AddSingleton(TimeProvider.System);
        services.AddAuditReadModelPersistence(settings.ReadModelConnection);
        services.AddDatabaseStartupInitializer();

        services.AddScoped<ICanonicalAuditEventPersister, CanonicalAuditEventPersister>();
        services.AddScoped<ContractSearchProjectionWriter>();
        services.AddScoped<IContractProjectionPersister, ContractProjectionPersister>();
        services.AddScoped<IAuditUnitOfWork>(provider => provider.GetRequiredService<AuditDbContext>());
    }
}
