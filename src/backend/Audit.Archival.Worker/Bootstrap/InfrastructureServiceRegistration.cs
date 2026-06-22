using Audit.Application.Archiving;
using Audit.Infrastructure.Archiving.Execution;
using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Archiving.Reactivation;
using Audit.Infrastructure.Archiving.Snapshots;
using Audit.Infrastructure.Archiving.Transfers;
using Audit.Infrastructure.Observability;
using Audit.Infrastructure.Persistence.Core;

namespace Audit.Archival.Worker.Bootstrap;

internal static class InfrastructureServiceRegistration
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration,
        ArchivalSettings settings)
    {
        services.AddAuditObservability(configuration, "audit-archival-worker");
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(settings.Options);
        services.AddAuditReadModelPersistence(settings.ReadModelConnection);
        services.AddAuditArchivePersistence(settings.ArchiveConnection);
        services.AddDatabaseStartupInitializer();
        services.AddScoped<ContractArchiveSnapshotStore>();
        services.AddScoped<ContractArchiveTransferStore>();
        services.AddScoped<ContractArchivalExecutor>();
        services.AddScoped<ArchivedContractRestorer>();
        services.AddScoped<ArchivedContractReactivator>();
        services.AddScoped<IContractArchiveLifecycle, DatabaseContractArchiveLifecycle>();
    }
}
