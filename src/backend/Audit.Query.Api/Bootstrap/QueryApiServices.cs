namespace Audit.Query.Api.Bootstrap;

internal static class QueryApiServices
{
    public static void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = InfrastructureSettings.From(configuration);

        OptionsServiceRegistration.Register(services);
        EndpointServiceRegistration.Register(services);
        InfrastructureServiceRegistration.Register(services, configuration, settings);
        MessagingServiceRegistration.Register(services, settings);
        HttpApiServiceRegistration.Register(services);
    }
}
