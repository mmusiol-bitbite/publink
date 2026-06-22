using Audit.Contracts;

namespace Audit.Application.Persistence;

public interface ICanonicalAuditEventPersister
{
    Task<AppendAuditEventResult> TryAppendAsync(
        AuditEntryImportedV1 message,
        CancellationToken cancellationToken);
}
