using Audit.Application.Legacy;
using Audit.Infrastructure.Configuration;

namespace Audit.Ingestion.Worker.Bootstrap;

internal sealed record IngestionSettings(
    LegacyImportOptions ImportOptions,
    string ReadModelConnection,
    string ServiceBusConnection,
    string ServiceBusAdministrationConnection,
    string LegacySourceConnection)
{
    public static IngestionSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var importOptions = configuration
            .GetSection(LegacyImportOptions.SectionName)
            .Get<LegacyImportOptions>() ?? new LegacyImportOptions();
        ValidateImportOptions(importOptions);

        var serviceBusConnection = configuration.GetRequiredConnectionString("ServiceBus");

        return new IngestionSettings(
            importOptions,
            configuration.GetRequiredConnectionString("ReadModel"),
            serviceBusConnection,
            configuration.GetConnectionString("ServiceBusAdministration")
                ?? serviceBusConnection,
            configuration.GetRequiredConnectionString("LegacySource"));
    }

    private static void ValidateImportOptions(LegacyImportOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Source)
            || options.BatchSize <= 0
            || options.PollingInterval <= TimeSpan.Zero
            || string.IsNullOrWhiteSpace(options.SourceTimeZone))
        {
            throw new InvalidOperationException("LegacyImport configuration is invalid.");
        }
    }
}
