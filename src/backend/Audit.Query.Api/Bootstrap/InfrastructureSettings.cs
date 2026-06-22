using Audit.Infrastructure.Configuration;

namespace Audit.Query.Api.Bootstrap;

internal sealed record InfrastructureSettings(
    string ReadModelConnection,
    string ArchiveConnection,
    string ServiceBusConnection,
    string ServiceBusAdministrationConnection)
{
    public static InfrastructureSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var serviceBusConnection = configuration.GetRequiredConnectionString("ServiceBus");
        return new InfrastructureSettings(
            configuration.GetRequiredConnectionString("ReadModel"),
            configuration.GetRequiredConnectionString("Archive"),
            serviceBusConnection,
            configuration.GetConnectionString("ServiceBusAdministration") ?? serviceBusConnection);
    }

}
