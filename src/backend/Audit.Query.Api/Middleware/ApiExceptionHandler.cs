using System.Diagnostics.CodeAnalysis;
using Audit.Application.Exports;
using Audit.Application.Legacy;
using Audit.Application.Queries;
using Microsoft.AspNetCore.Diagnostics;

namespace Audit.Query.Api.Middleware;

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated by the ASP.NET Core exception-handler registration.")]
internal sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "invalidRequest"),
            ExportTooLargeException => (StatusCodes.Status413PayloadTooLarge, "exportTooLarge"),
            LegacySynchronizationUnavailableException =>
                (StatusCodes.Status503ServiceUnavailable, "synchronizationUnavailable"),
            ContractStoreUnavailableException =>
                (StatusCodes.Status503ServiceUnavailable, "contractStoreUnavailable"),
            _ => (StatusCodes.Status500InternalServerError, "unexpectedError")
        };

        await Results.Problem(
            statusCode: status,
            title: title,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            }).ExecuteAsync(httpContext);
        return true;
    }
}
