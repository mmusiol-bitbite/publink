using System.Diagnostics.CodeAnalysis;

namespace Audit.Query.Api.Middleware;

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated by the ASP.NET Core middleware pipeline.")]
internal sealed class ResponseSecurityHeadersMiddleware(
    RequestDelegate next,
    ILogger<ResponseSecurityHeadersMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        using var scope = logger.BeginScope(
            new Dictionary<string, object?> { ["TraceId"] = context.TraceIdentifier });
        await next(context);
    }
}
