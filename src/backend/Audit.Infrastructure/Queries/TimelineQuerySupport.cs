using System.Globalization;
using System.Text;
using System.Text.Json;
using Audit.Application.Queries;

namespace Audit.Infrastructure.Queries;

internal static class TimelineQuerySupport
{
    // A valid cursor encodes two ulong values.
    // Maximum raw value: "18446744073709551615:18446744073709551615" (41 chars),
    // which becomes ~56 chars when Base64-encoded.
    private const int MaxCursorLength = 128;

    public static TimelineCursor? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        if (cursor.Length > MaxCursorLength)
        {
            throw new ArgumentException("The timeline cursor is invalid.", nameof(cursor));
        }

        try
        {
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Split(':', 2);
            return parts.Length == 2 &&
                   long.TryParse(parts[0], CultureInfo.InvariantCulture, out var snapshot) &&
                   long.TryParse(parts[1], CultureInfo.InvariantCulture, out var before)
                ? new TimelineCursor(snapshot, before)
                : throw new FormatException("Invalid timeline cursor.");
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException)
        {
            throw new ArgumentException("The timeline cursor is invalid.", nameof(cursor), exception);
        }
    }

    public static string EncodeCursor(long snapshot, long before) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{snapshot}:{before}")));

    public static TimelineItem Map(TimelineRow row) => new(
        row.EventId,
        row.SourceSequence,
        row.OccurredAt,
        row.CorrelationId,
        row.ChangeKind,
        row.ChangeKindCode,
        row.EntityKind,
        row.EntityKindCode,
        row.Actor,
        JsonSerializer.Deserialize<TimelineFieldChange[]>(row.ChangesJson) ?? [],
        JsonSerializer.Deserialize<string[]>(row.DataQualityIssuesJson) ?? []);
}

internal sealed record TimelineCursor(long SnapshotSequence, long BeforeSequence);

internal sealed record TimelineRow(
    Guid EventId,
    long SourceSequence,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    string ChangeKind,
    int ChangeKindCode,
    string EntityKind,
    int EntityKindCode,
    string Actor,
    string ChangesJson,
    string DataQualityIssuesJson);
