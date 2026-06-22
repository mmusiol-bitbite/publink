using System.Text.Json;
using Audit.Application.Persistence;
using Audit.Contracts;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Persisters;

public sealed class CanonicalAuditEventPersister(AuditDbContext dbContext)
    : ICanonicalAuditEventPersister
{
    public async Task<AppendAuditEventResult> TryAppendAsync(
        AuditEntryImportedV1 message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var exists = await dbContext.AuditEvents.AnyAsync(
            item => item.Source == message.Source && item.SourceEventId == message.SourceEventId,
            cancellationToken);

        if (exists)
        {
            return AppendAuditEventResult.Duplicate;
        }

        dbContext.AuditEvents.Add(new CanonicalAuditEventEntity
        {
            EventId = message.EventId,
            Source = message.Source,
            SourceEventId = message.SourceEventId,
            SourceSequence = message.SourceSequence,
            OrganizationId = message.OrganizationId,
            OccurredAt = message.OccurredAt,
            IngestedAt = message.IngestedAt,
            ActorId = message.Actor.UserId,
            ActorEmail = message.Actor.Email,
            CorrelationId = message.Operation.CorrelationId,
            ChangeTypeCode = message.Operation.ChangeTypeCode,
            EntityTypeCode = message.Entity.EntityTypeCode,
            EntityId = message.Entity.EntityId,
            ParentId = message.Entity.ParentId,
            PrimaryKey = message.Entity.PrimaryKey,
            BeforeJson = message.Before?.GetRawText(),
            AfterJson = message.After?.GetRawText(),
            ChangedFieldsJson = JsonSerializer.Serialize(message.ChangedFields)
        });

        return AppendAuditEventResult.Appended;
    }
}
