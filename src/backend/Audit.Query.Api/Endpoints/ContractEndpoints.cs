using Audit.Application.Queries;

namespace Audit.Query.Api.Endpoints;

internal static class ContractEndpoints
{
    private static readonly ContractEndpointNames Names = new(
        "SearchContracts",
        "GetContractAuditTimeline",
        "ExportContractAudit");

    public static IEndpointRouteBuilder MapContractEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapContractEndpointSet(
            "/api/v1/contracts",
            Names,
            ContractAuditDataSource.Active);
}
