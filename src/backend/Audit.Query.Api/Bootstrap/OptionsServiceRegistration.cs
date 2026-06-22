using Audit.Query.Api.Configuration;

namespace Audit.Query.Api.Bootstrap;

internal static class OptionsServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        services
            .AddOptions<SynchronizationOptions>()
            .BindConfiguration(SynchronizationOptions.SectionName)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Source),
                "Synchronization:Source is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DeadLetterQueue),
                "Synchronization:DeadLetterQueue is required.")
            .Validate(
                options => options.HealthyStatusMaxAge > TimeSpan.Zero,
                "Synchronization:HealthyStatusMaxAge must be greater than zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.LegacySynchronizationQueueName),
                "Synchronization:LegacySynchronizationQueueName is required.")
            .ValidateOnStart();

        services
            .AddOptions<QueryCorsOptions>()
            .BindConfiguration(QueryCorsOptions.SectionName)
            .Validate(
                options => options.AllowedOrigins.All(IsValidCorsOrigin),
                "Every Cors:AllowedOrigins entry must be an absolute HTTP or HTTPS URI.")
            .ValidateOnStart();
    }

    private static bool IsValidCorsOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
