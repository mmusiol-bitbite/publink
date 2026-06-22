using Audit.Application.Archiving;
using Audit.Infrastructure.Configuration;

namespace Audit.Archival.Worker.Bootstrap;

internal sealed record ArchivalSettings(
    string ReadModelConnection,
    string ArchiveConnection,
    ContractArchivalOptions Options)
{
    public static ArchivalSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration
            .GetSection(ContractArchivalOptions.SectionName)
            .Get<ContractArchivalOptions>() ?? new ContractArchivalOptions();
        if (options.InactivityMonths <= 0
            || options.RunInterval <= TimeSpan.Zero
            || options.MaximumContractsPerRun <= 0)
        {
            throw new InvalidOperationException("Archival configuration values must be positive.");
        }

        return new ArchivalSettings(
            configuration.GetRequiredConnectionString("ReadModel"),
            configuration.GetRequiredConnectionString("Archive"),
            options);
    }

}
