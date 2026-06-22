using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Audit.Query.Api.Endpoints;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }))
            .ExcludeFromDescription();
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteReadinessResponseAsync
        })
            .ExcludeFromDescription();

        return endpoints;
    }

    private static Task WriteReadinessResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        var status = report.Status == HealthStatus.Healthy ? "ready" : "notReady";
        context.Response.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            new { status },
            cancellationToken: context.RequestAborted);
    }
}
