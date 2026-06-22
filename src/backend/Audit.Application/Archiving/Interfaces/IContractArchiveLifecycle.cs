namespace Audit.Application.Archiving;

public interface IContractArchiveLifecycle
{
    Task<ContractArchivalResult> RunOnceAsync(CancellationToken cancellationToken);
}
