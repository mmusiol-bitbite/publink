using Audit.Application.Legacy;
using Audit.Application.Queries;
using Audit.Contracts;
using Audit.Query.Api.Bootstrap;
using Audit.Query.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Audit.Query.Api.Endpoints;

internal static class SynchronizationEndpoints
{
    public static IEndpointRouteBuilder MapSynchronizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/synchronization/status", GetStatusAsync)
            .WithName("GetSynchronizationStatus");

        endpoints.MapPost("/api/v1/synchronization/requests", RequestSynchronizationAsync)
            .WithName("RequestLegacySynchronization")
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .RequireRateLimiting(RateLimitingConfiguration.PolicyName);

        return endpoints;
    }

    private static async Task<IResult> GetStatusAsync(
        ISynchronizationStatusReader reader,
        IOptions<SynchronizationOptions> options,
        CancellationToken cancellationToken) =>
            Results.Ok(await reader.ReadAsync(
                options.Value.Source,
                options.Value.HealthyStatusMaxAge,
                cancellationToken));

    private static async Task<IResult> RequestSynchronizationAsync(
        ILegacySynchronizationRequester requester,
        ILegacySynchronizationRequestStore requestStore,
        IOptions<SynchronizationOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var lease = await requestStore.AcquireAsync(
            options.Value.Source,
            timeProvider.GetUtcNow(),
            cancellationToken);
        if (lease.Created)
        {
            var command = new RequestLegacySynchronizationV1(
                lease.CorrelationId,
                lease.Source,
                lease.RequestedAt);
            try
            {
                await requester.RequestAsync(command, cancellationToken);
            }
            catch
            {
                await requestStore.ReleaseAsync(lease.CorrelationId, CancellationToken.None);
                throw;
            }
        }

        return Results.Accepted(value: new
        {
            requestId = lease.CorrelationId,
            lease.Source,
            acceptedAt = lease.RequestedAt,
            status = "Accepted",
            joined = !lease.Created
        });
    }
}
