using Audit.Domain;
using Audit.Query.Api.Endpoints.Requests;

namespace Audit.Query.Api.Endpoints.Validation;

internal sealed class TimelineQueryValidator : IRequestValidator<ITimelineFilterQuery>
{
    public ValidationResult Validate(ITimelineFilterQuery request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new ValidationResult();
        if (request.From > request.To)
        {
            result.Add("dateRange", "timelineDateRangeInvalid");
        }

        if (request.Actor?.Length > 200)
        {
            result.Add("actor", "timelineActorTooLong");
        }

        if (request.ChangeType is { } changeType && !ChangeKind.IsKnownCode(changeType))
        {
            result.Add("changeType", "timelineChangeTypeInvalid");
        }

        if (request.EntityType is { } entityType && !AuditedEntityKind.IsKnownCode(entityType))
        {
            result.Add("entityType", "timelineEntityTypeInvalid");
        }

        return result;
    }
}
