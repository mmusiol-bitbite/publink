namespace Audit.Domain;

public sealed record FieldChange(
    string Field,
    string? Before,
    string? After,
    string ValueKind);

public sealed record ChangeSet(
    IReadOnlyList<FieldChange> Changes,
    IReadOnlyList<string> DataQualityIssues);

