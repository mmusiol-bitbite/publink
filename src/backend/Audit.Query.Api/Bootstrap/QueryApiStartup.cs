using Audit.Query.Api.Endpoints;
using Audit.Query.Api.Middleware;

namespace Audit.Query.Api.Bootstrap;

internal static class QueryApiStartup
{
    public static void ConfigurePipeline(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseMiddleware<ResponseSecurityHeadersMiddleware>();
        app.UseCors();
        app.UseRateLimiter();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    public static void MapEndpoints(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthEndpoints();
        app.MapSynchronizationEndpoints();
        app.MapContractEndpoints();
        app.MapArchivedContractEndpoints();
    }
}
