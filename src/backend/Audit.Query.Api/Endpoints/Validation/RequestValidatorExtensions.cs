namespace Audit.Query.Api.Endpoints.Validation;

internal static class RequestValidatorExtensions
{
    public static bool TryValidateAsProblem<T>(
        this IRequestValidator<T> validator,
        T instance,
        out IResult? problem)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var validation = validator.Validate(instance);
        if (validation.IsValid)
        {
            problem = null;
            return true;
        }

        problem = Results.ValidationProblem(validation.ToProblemDetailsErrors());
        return false;
    }
}
