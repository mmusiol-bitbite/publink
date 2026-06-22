namespace Audit.Application.Queries;

public interface IContractStore : IContractSearchSource, IContractTimelineSource
{
    ContractAuditDataSource DataSource { get; }
}
