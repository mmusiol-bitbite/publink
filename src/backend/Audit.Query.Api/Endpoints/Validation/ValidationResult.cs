namespace Audit.Query.Api.Endpoints.Validation;

internal sealed class ValidationResult
{
    private readonly Dictionary<string, List<string>> errors = [];

    public bool IsValid => errors.Count == 0;

    public void Add(string field, string errorCode)
    {
        if (!errors.TryGetValue(field, out var fieldErrors))
        {
            fieldErrors = [];
            errors[field] = fieldErrors;
        }

        fieldErrors.Add(errorCode);
    }

    public Dictionary<string, string[]> ToProblemDetailsErrors() =>
        errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
}
