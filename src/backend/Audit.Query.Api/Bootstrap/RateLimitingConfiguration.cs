using System.Threading.RateLimiting;
using Audit.Query.Api.Configuration;
using Microsoft.AspNetCore.RateLimiting;

namespace Audit.Query.Api.Bootstrap;

internal static class RateLimitingConfiguration
{
    public const string PolicyName = "manual-synchronization";

    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
            _ => RateLimitPartition.GetFixedWindowLimiter(
                "global",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.Headers["Retry-After"] = "60";
            await context.HttpContext.Response.WriteAsJsonAsync(
                new
                {
                    type = "about:blank",
                    title = "rateLimitExceeded",
                    status = StatusCodes.Status429TooManyRequests,
                    traceId = context.HttpContext.TraceIdentifier
                },
                cancellationToken);
        };
        options.AddFixedWindowLimiter(PolicyName, limiter =>
        {
            limiter.PermitLimit = 30;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
            limiter.AutoReplenishment = true;
        });
    }
}
