namespace Audit.Application.Legacy;

public sealed class LegacyImportOptions
{
    public const string SectionName = "LegacyImport";

    public string Source { get; init; } = "contracts-sql";

    public int BatchSize { get; init; } = 500;

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromHours(1);

    public string SourceTimeZone { get; init; } = "UTC";
}

