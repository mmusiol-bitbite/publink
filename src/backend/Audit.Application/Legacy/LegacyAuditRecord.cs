namespace Audit.Application.Legacy;

public sealed record LegacyAuditRecord(
    int SourceEventId,
    Guid? OrganizationId,
    Guid? UserId,
    string? UserEmail,
    int Type,
    int EntityType,
    DateTime CreatedDate,
    string? OldValues,
    string? NewValues,
    string? AffectedColumns,
    string? PrimaryKey,
    Guid? EntityId,
    Guid? ParentId,
    Guid CorrelationId);

