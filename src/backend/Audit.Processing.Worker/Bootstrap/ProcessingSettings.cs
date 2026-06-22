using Audit.Infrastructure.Configuration;

namespace Audit.Processing.Worker.Bootstrap;

internal sealed record ProcessingSettings(
    string ReadModelConnection,
    string ServiceBusConnection,
    string ServiceBusAdministrationConnection)
{
    public static ProcessingSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var serviceBusConnection = configuration.GetRequiredConnectionString("ServiceBus");
        return new ProcessingSettings(
            configuration.GetRequiredConnectionString("ReadModel"),
            serviceBusConnection,
            configuration.GetConnectionString("ServiceBusAdministration")
                ?? serviceBusConnection);
    }

}
