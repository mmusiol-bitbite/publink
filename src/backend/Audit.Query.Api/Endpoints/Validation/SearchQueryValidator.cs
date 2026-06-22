using Audit.Query.Api.Endpoints.Requests;

namespace Audit.Query.Api.Endpoints.Validation;

internal sealed class SearchQueryValidator : IRequestValidator<SearchQuery>
{
    public ValidationResult Validate(SearchQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new ValidationResult();
        var query = request.SearchPhrase?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length < 2)
        {
            result.Add("searchPhrase", "searchQueryTooShort");
        }
        else if (query.Length > 200)
        {
            result.Add("searchPhrase", "searchQueryTooLong");
        }

        return result;
    }
}
