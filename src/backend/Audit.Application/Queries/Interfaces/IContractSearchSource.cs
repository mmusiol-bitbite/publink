namespace Audit.Application.Queries;

public interface IContractSearchSource
{
    Task<IReadOnlyList<ContractSearchResult>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken);
}
