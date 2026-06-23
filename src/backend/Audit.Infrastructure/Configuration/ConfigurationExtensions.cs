using Microsoft.Extensions.Configuration;

namespace Audit.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static string GetRequiredConnectionString(
        this IConfiguration configuration,
        string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"ConnectionStrings:{name} is required.");
    }
}
