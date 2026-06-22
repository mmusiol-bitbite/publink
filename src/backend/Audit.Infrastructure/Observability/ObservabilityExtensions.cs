using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Audit.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    // The standard OpenTelemetry environment variable for the OTLP exporter endpoint.
    // Reading it via IConfiguration supports overriding through appsettings or env vars.
    private const string OtlpEndpointConfigKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static IServiceCollection AddAuditObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool instrumentAspNetCore = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var endpoint = configuration[OtlpEndpointConfigKey];
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("MassTransit")
                    .AddHttpClientInstrumentation();
                if (instrumentAspNetCore)
                {
                    tracing.AddAspNetCoreInstrumentation(options =>
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(
                                "/health",
                                StringComparison.OrdinalIgnoreCase));
                }

                if (Uri.TryCreate(endpoint, UriKind.Absolute, out var tracingEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = tracingEndpoint);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation();
                if (instrumentAspNetCore)
                {
                    metrics.AddAspNetCoreInstrumentation();
                }
                if (Uri.TryCreate(endpoint, UriKind.Absolute, out var metricsEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = metricsEndpoint);
                }
            });

        return services;
    }
}
