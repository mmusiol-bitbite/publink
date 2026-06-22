using System.Security.Cryptography;
using System.Text;
using Audit.Contracts;

namespace Audit.Application.Legacy;

public sealed class LegacyAuditEventMapper(TimeProvider timeProvider, LegacyImportOptions options)
{
    public AuditEntryImportedV1 Map(LegacyAuditRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new AuditEntryImportedV1(
            GenerateDeterministicEventId(options.Source, record.SourceEventId),
            1,
            options.Source,
            record.SourceEventId,
            record.SourceEventId,
            record.OrganizationId,
            record.CreatedDate.ToUtcOffset(options.SourceTimeZone),
            timeProvider.GetUtcNow(),
            new ActorSnapshotV1(record.UserId, record.UserEmail),
            new AuditOperationV1(record.CorrelationId, record.Type),
            new AuditedEntityV1(record.EntityType, record.EntityId, record.ParentId, record.PrimaryKey),
            record.OldValues.ParseAsNullableJsonElement(),
            record.NewValues.ParseAsNullableJsonElement(),
            record.AffectedColumns.ParseAsStringArray());
    }

    private static Guid GenerateDeterministicEventId(string source, long sourceEventId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{source}:{sourceEventId}"));
        Span<byte> guidBytes = stackalloc byte[16];
        bytes.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
