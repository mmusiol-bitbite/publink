using System.Text.Json;

namespace Audit.Domain;

public static class FieldChangeFactory
{
    public static ChangeSet Create(
        string? beforeJson,
        string? afterJson,
        IReadOnlyCollection<string>? affectedFields = null)
    {
        var issues = new List<string>();
        var before = ParseObject(beforeJson, "invalidBeforeJson", issues);
        var after = ParseObject(afterJson, "invalidAfterJson", issues);

        var fields = affectedFields is { Count: > 0 }
            ? affectedFields
            : before.Keys.Union(after.Keys, StringComparer.OrdinalIgnoreCase).ToArray();

        var changes = fields
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(field => Build(field, before, after))
            .Where(change => change.Before != change.After)
            .ToArray();

        return new ChangeSet(changes, issues);
    }

    private static FieldChange Build(
        string field,
        Dictionary<string, JsonElement> before,
        Dictionary<string, JsonElement> after)
    {
        var hasBefore = before.TryGetValue(field, out var beforeValue);
        var hasAfter = after.TryGetValue(field, out var afterValue);
        var valueKind = hasAfter
            ? ToValueKindToken(afterValue.ValueKind)
            : hasBefore
                ? ToValueKindToken(beforeValue.ValueKind)
                : "unknown";

        return new FieldChange(
            field,
            hasBefore ? ToInvariantString(beforeValue) : null,
            hasAfter ? ToInvariantString(afterValue) : null,
            valueKind);
    }

    private static Dictionary<string, JsonElement> ParseObject(
        string? json,
        string issue,
        List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                issues.Add(issue);
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            return document.RootElement
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => property.Value.Clone(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            issues.Add(issue);
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string? ToInvariantString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => value.GetRawText()
    };

    private static string ToValueKindToken(JsonValueKind valueKind) => valueKind switch
    {
        JsonValueKind.Array => "array",
        JsonValueKind.False => "false",
        JsonValueKind.Null => "null",
        JsonValueKind.Number => "number",
        JsonValueKind.Object => "object",
        JsonValueKind.String => "string",
        JsonValueKind.True => "true",
        JsonValueKind.Undefined => "undefined",
        _ => "unknown"
    };
}
