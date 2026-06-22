using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Query.Api.Endpoints.Requests;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by the ASP.NET Core parameter binder.")]
internal sealed class TimelineQuery : ITimelineFilterQuery
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 100;

    [FromQuery(Name = "limit")]
    public int? Limit { get; init; }

    [FromQuery(Name = "cursor")]
    public string? Cursor { get; init; }

    [FromQuery(Name = "from")]
    public DateTimeOffset? From { get; init; }

    [FromQuery(Name = "to")]
    public DateTimeOffset? To { get; init; }

    [FromQuery(Name = "actor")]
    public string? Actor { get; init; }

    [FromQuery(Name = "changeType")]
    public int? ChangeType { get; init; }

    [FromQuery(Name = "entityType")]
    public int? EntityType { get; init; }
}
