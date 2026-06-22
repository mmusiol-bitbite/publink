namespace Audit.Application.Queries;

public sealed record TimelineFilter(
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Actor,
    int? ChangeType,
    int? EntityType);
