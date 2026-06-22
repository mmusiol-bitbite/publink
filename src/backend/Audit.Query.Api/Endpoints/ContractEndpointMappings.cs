using Audit.Application.Exports;
using Audit.Application.Queries;
using Audit.Query.Api.Endpoints.Requests;
using Audit.Query.Api.Endpoints.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Query.Api.Endpoints;

internal static class ContractEndpointMappings
{
    public static IEndpointRouteBuilder MapContractEndpointSet(
        this IEndpointRouteBuilder endpoints,
        string routePrefix,
        ContractEndpointNames names,
        ContractAuditDataSource dataSource)
    {
        var group = endpoints.MapGroup(routePrefix);

        group.MapGet("/search", (
                [AsParameters] SearchQuery query,
                IRequestValidator<SearchQuery> validator,
                HttpContext http,
                CancellationToken cancellationToken) =>
                ContractEndpointHandlers.SearchAsync(
                    query,
                    validator,
                    ResolveSource(http, dataSource),
                    cancellationToken))
            .WithName(names.Search)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet(
                "/{contractId:guid}/audit-events",
                (
                    Guid contractId,
                    [AsParameters] TimelineQuery query,
                    IRequestValidator<ITimelineFilterQuery> validator,
                    HttpContext http,
                    CancellationToken cancellationToken) =>
                    ContractEndpointHandlers.GetTimelineAsync(
                        contractId,
                        query,
                        validator,
                        ResolveSource(http, dataSource),
                        cancellationToken))
            .WithName(names.Timeline)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet(
                "/{contractId:guid}/audit-events/export",
                (
                    Guid contractId,
                    [AsParameters] ExportQuery query,
                    IRequestValidator<ExportQuery> validator,
                    HttpContext http,
                    ContractAuditExportService exportService,
                    CancellationToken cancellationToken) =>
                    ContractEndpointHandlers.ExportAsync(
                        contractId,
                        query,
                        validator,
                        http,
                        exportService,
                        ResolveSource(http, dataSource),
                        cancellationToken))
            .WithName(names.Export)
            .Produces(StatusCodes.Status200OK, contentType: "application/zip")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static IContractStore ResolveSource(
        HttpContext http,
        ContractAuditDataSource dataSource) =>
        http.RequestServices.GetRequiredKeyedService<IContractStore>(dataSource);
}

internal sealed record ContractEndpointNames(string Search, string Timeline, string Export);
