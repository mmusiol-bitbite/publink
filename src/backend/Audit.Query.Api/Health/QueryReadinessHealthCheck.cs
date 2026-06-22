using System.Diagnostics.CodeAnalysis;
using Audit.Infrastructure.Persistence.Core;
using Audit.Query.Api.Configuration;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Audit.Query.Api.Health;

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated by the ASP.NET Core health-check registration.")]
internal sealed class QueryReadinessHealthCheck(
    SqlConnectionFactory connections,
    ServiceBusAdministrationClient serviceBusAdministration,
    IOptions<SynchronizationOptions> synchronizationOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        await command.ExecuteScalarAsync(cancellationToken);

        var synchronizationQueue = await serviceBusAdministration.QueueExistsAsync(
            synchronizationOptions.Value.LegacySynchronizationQueueName,
            cancellationToken);

        return synchronizationQueue.Value
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Legacy synchronization queue does not exist.");
    }
}
