using MassTransit.AzureServiceBusTransport;

namespace Audit.Infrastructure.Messaging;

public static class ServiceBusEmulatorCompatibility
{
    public static void ApplyIfRequired(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        if (!connectionString.Contains("UseDevelopmentEmulator=true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Defaults.DefaultMessageTimeToLive = TimeSpan.FromHours(1);
        Defaults.BasicMessageTimeToLive = TimeSpan.FromHours(1);
        Defaults.AutoDeleteOnIdle = TimeSpan.FromHours(1);
    }
}
