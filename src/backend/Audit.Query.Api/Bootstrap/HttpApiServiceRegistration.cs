using Audit.Query.Api.Configuration;
using Audit.Query.Api.Health;
using Audit.Query.Api.Middleware;
using Microsoft.Extensions.Options;

namespace Audit.Query.Api.Bootstrap;

internal static class HttpApiServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddHealthChecks()
            .AddCheck<QueryReadinessHealthCheck>("query-api-readiness");
        services.AddRateLimiter(RateLimitingConfiguration.Configure);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors();
        services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
            .Configure<IOptions<QueryCorsOptions>>((options, queryCorsOptions) =>
                options.AddDefaultPolicy(policy =>
                {
                    if (queryCorsOptions.Value.AllowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(queryCorsOptions.Value.AllowedOrigins);
                    }

                    policy.AllowAnyHeader().AllowAnyMethod();
                }));
    }
}
