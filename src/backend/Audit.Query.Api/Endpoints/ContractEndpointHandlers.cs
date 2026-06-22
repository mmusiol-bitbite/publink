using Audit.Application.Exports;
using Audit.Application.Queries;
using Audit.Query.Api.Endpoints.Requests;
using Audit.Query.Api.Endpoints.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Query.Api.Endpoints;

internal static class ContractEndpointHandlers
{
    public static async Task<IResult> SearchAsync(
        [AsParameters] SearchQuery query,
        IRequestValidator<SearchQuery> validator,
        IContractStore source,
        CancellationToken cancellationToken)
    {
        if (!validator.TryValidateAsProblem(query, out var problem))
        {
            return problem!;
        }

        var results = await source.SearchAsync(
            query.SearchPhrase!.Trim(),
            Math.Clamp(query.Limit ?? SearchQuery.DefaultLimit, 1, SearchQuery.MaxLimit),
            cancellationToken);

        return Results.Ok(new { items = results });
    }

    public static async Task<IResult> GetTimelineAsync(
        Guid contractId,
        [AsParameters] TimelineQuery query,
        IRequestValidator<ITimelineFilterQuery> validator,
        IContractStore source,
        CancellationToken cancellationToken)
    {
        if (!validator.TryValidateAsProblem(query, out var problem))
        {
            return problem!;
        }

        return Results.Ok(await source.ReadAsync(
            contractId,
            Math.Clamp(query.Limit ?? TimelineQuery.DefaultLimit, 1, TimelineQuery.MaxLimit),
            query.Cursor,
            CreateTimelineFilter(query),
            cancellationToken));
    }

    public static async Task<IResult> ExportAsync(
        Guid contractId,
        [AsParameters] ExportQuery query,
        IRequestValidator<ExportQuery> validator,
        HttpContext http,
        ContractAuditExportService export,
        IContractStore source,
        CancellationToken cancellationToken)
    {
        if (!validator.TryValidateAsProblem(query, out var problem))
        {
            return problem!;
        }

        var package = await export.GenerateAsync(
            source,
            source.DataSource,
            contractId,
            CreateTimelineFilter(query),
            query.Locale ?? "pl",
            cancellationToken);
        http.Response.Headers["X-Content-SHA256"] = package.Sha256;

        return Results.File(package.Content.ToArray(), "application/zip", package.FileName);
    }

    private static TimelineFilter CreateTimelineFilter(ITimelineFilterQuery query) => new(
        query.From,
        query.To,
        query.Actor?.Trim(),
        query.ChangeType,
        query.EntityType);
}
