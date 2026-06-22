namespace Audit.Query.Api.Endpoints.Validation;

internal interface IRequestValidator<in TRequest>
{
    ValidationResult Validate(TRequest request);
}
