using System.Text.Json;

namespace Audit.Contracts;

public sealed record AuditEntryImportedV1(
    Guid EventId,
    int SchemaVersion,
    string Source,
    long SourceEventId,
    long SourceSequence,
    Guid? OrganizationId,
    DateTimeOffset OccurredAt,
    DateTimeOffset IngestedAt,
    ActorSnapshotV1 Actor,
    AuditOperationV1 Operation,
    AuditedEntityV1 Entity,
    JsonElement? Before,
    JsonElement? After,
    IReadOnlyList<string> ChangedFields) : AuditContract;

public sealed record ActorSnapshotV1(Guid? UserId, string? Email);

public sealed record AuditOperationV1(Guid CorrelationId, int ChangeTypeCode);

public sealed record AuditedEntityV1(
    int EntityTypeCode,
    Guid? EntityId,
    Guid? ParentId,
    string? PrimaryKey);
