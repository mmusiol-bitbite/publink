using Audit.Contracts;

namespace Audit.Application.Persistence;

public interface IContractProjectionPersister
{
    Task ApplyAsync(AuditEntryImportedV1 message, CancellationToken cancellationToken);
}
