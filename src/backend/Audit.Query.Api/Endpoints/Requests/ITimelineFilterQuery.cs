namespace Audit.Query.Api.Endpoints.Requests;

internal interface ITimelineFilterQuery
{
    DateTimeOffset? From { get; }

    DateTimeOffset? To { get; }

    string? Actor { get; }

    int? ChangeType { get; }

    int? EntityType { get; }
}
