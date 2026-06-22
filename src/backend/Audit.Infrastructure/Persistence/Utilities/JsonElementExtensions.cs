using System.Text.Json;

namespace Audit.Infrastructure.Persistence.Utilities;

internal static class JsonElementExtensions
{
    /// <summary>
    /// Flattens a JSON object element into a case-insensitive field dictionary.
    /// Returns an empty dictionary when the element is null or not a JSON object.
    /// </summary>
    public static Dictionary<string, JsonElement> ReadFields(this JsonElement? element)
    {
        if (element is not { ValueKind: JsonValueKind.Object } obj)
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        return obj.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.Clone(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the string value for <paramref name="fieldName"/>, or <c>null</c> when not present.
    /// Non-string JSON values are returned as raw text.
    /// </summary>
    public static string? ReadFieldValue(this Dictionary<string, JsonElement> fields, string fieldName) =>
        fields.TryGetValue(fieldName, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.String => NormalizeString(value.GetString()),
                _ => value.GetRawText()
            }
            : null;

    private static string? NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Equals("null", StringComparison.OrdinalIgnoreCase)
            ? null
            : value;
    }
}
