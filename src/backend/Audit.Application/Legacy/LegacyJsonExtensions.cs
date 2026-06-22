using System.Text.Json;

namespace Audit.Application.Legacy;

internal static class LegacyJsonExtensions
{
    /// <summary>
    /// Parses a JSON string into a <see cref="JsonElement"/>. Returns <c>null</c> when the input
    /// is null, empty, or malformed.
    /// </summary>
    public static JsonElement? ParseAsNullableJsonElement(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a JSON array of strings. Returns an empty array when the input is null, empty,
    /// not a valid JSON array, or malformed.
    /// </summary>
    public static string[] ParseAsStringArray(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Array
                ? [.. document.RootElement
                    .EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString())
                    .OfType<string>()]
                : [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
