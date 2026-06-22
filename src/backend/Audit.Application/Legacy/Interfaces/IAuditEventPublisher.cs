using Audit.Contracts;

namespace Audit.Application.Legacy;

public interface IAuditEventPublisher
{
    Task PublishAsync(AuditEntryImportedV1 message, CancellationToken cancellationToken);
}
