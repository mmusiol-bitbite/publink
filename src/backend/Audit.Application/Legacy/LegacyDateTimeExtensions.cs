namespace Audit.Application.Legacy;

internal static class LegacyDateTimeExtensions
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> from the given source timezone to UTC as a <see cref="DateTimeOffset"/>.
    /// </summary>
    public static DateTimeOffset ToUtcOffset(this DateTime value, string sourceTimeZone)
    {
        if (string.Equals(sourceTimeZone, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
        }

        var timezone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZone);
        var unspecified = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timezone);
    }
}
