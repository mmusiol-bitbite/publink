namespace Audit.Application.Queries;

public sealed record TimelineItem(
    Guid EventId,
    long SourceSequence,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    string ChangeKind,
    int ChangeKindCode,
    string EntityKind,
    int EntityKindCode,
    string Actor,
    IReadOnlyList<TimelineFieldChange> Changes,
    IReadOnlyList<string> DataQualityIssues);
