using Audit.Query.Api.Endpoints.Requests;

namespace Audit.Query.Api.Endpoints.Validation;

internal sealed class ExportQueryValidator(
    IRequestValidator<ITimelineFilterQuery> timelineValidator)
    : IRequestValidator<ExportQuery>
{
    private static readonly HashSet<string> SupportedLocales =
        new(StringComparer.OrdinalIgnoreCase) { "pl", "en" };

    public ValidationResult Validate(ExportQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = timelineValidator.Validate(request);
        if (request.Locale is not null && !SupportedLocales.Contains(request.Locale))
        {
            result.Add("locale", "exportLocaleInvalid");
        }

        return result;
    }
}
