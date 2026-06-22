using Audit.Application.Legacy;
using Audit.Contracts;
using MassTransit;

namespace Audit.Infrastructure.Messaging;

public sealed class MassTransitAuditEventPublisher(IPublishEndpoint publishEndpoint) : IAuditEventPublisher
{
    public Task PublishAsync(AuditEntryImportedV1 message, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(message, cancellationToken);
}

