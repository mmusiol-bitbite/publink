using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Query.Api.Endpoints.Requests;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by the ASP.NET Core parameter binder.")]
internal sealed class SearchQuery
{
    public const int MaxLimit = 50;
    public const int DefaultLimit = 20;

    [FromQuery(Name = "searchPhrase")]
    public string? SearchPhrase { get; init; }

    [FromQuery(Name = "limit")]
    public int? Limit { get; init; }
}
