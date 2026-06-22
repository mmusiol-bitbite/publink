namespace Audit.Application.Queries;

public sealed record TimelineFieldChange(
    string Field,
    string? Before,
    string? After,
    string ValueKind);
