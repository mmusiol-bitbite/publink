using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure.Persistence.Core;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAuditReadModelPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<AuditDatabaseInitializer>();
        services.AddScoped<IDatabaseInitializer>(provider =>
            provider.GetRequiredService<AuditDatabaseInitializer>());
        return services;
    }

    public static IServiceCollection AddAuditArchivePersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ArchiveDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<ArchiveDatabaseInitializer>();
        services.AddScoped<IDatabaseInitializer>(provider =>
            provider.GetRequiredService<ArchiveDatabaseInitializer>());
        return services;
    }

    public static IServiceCollection AddDatabaseStartupInitializer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<DatabaseStartupInitializer>();
        return services;
    }

    public static Task InitializeRequiredDatabasesAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.GetRequiredService<DatabaseStartupInitializer>()
            .InitializeAsync(cancellationToken);
    }

}
