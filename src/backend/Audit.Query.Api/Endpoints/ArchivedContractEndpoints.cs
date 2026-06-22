using Audit.Application.Queries;

namespace Audit.Query.Api.Endpoints;

internal static class ArchivedContractEndpoints
{
    private static readonly ContractEndpointNames Names = new(
        "SearchArchivedContracts",
        "GetArchivedContractAuditTimeline",
        "ExportArchivedContractAudit");

    public static IEndpointRouteBuilder MapArchivedContractEndpoints(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapContractEndpointSet(
            "/api/v1/archive/contracts",
            Names,
            ContractAuditDataSource.Archive);
}
