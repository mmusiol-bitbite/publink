using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Audit.Application.Queries;

namespace Audit.Application.Exports;

public sealed class ContractAuditExportService(TimeProvider timeProvider)
{
    private const int MaximumEvents = 10_000;
    private static readonly JsonSerializerOptions ManifestJsonOptions =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<AuditExportPackage> GenerateAsync(
        IContractTimelineSource timelineSource,
        ContractAuditDataSource dataSource,
        Guid contractId,
        TimelineFilter filter,
        string locale,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(timelineSource);

        var normalizedLocale = "en".Equals(locale, StringComparison.OrdinalIgnoreCase) ? "en" : "pl";
        var page = await timelineSource.ReadAsync(
            contractId,
            MaximumEvents,
            null,
            filter,
            cancellationToken);

        if (page.NextCursor is not null)
        {
            throw new ExportTooLargeException(MaximumEvents);
        }

        var csv = CreateCsv(contractId, page.Items, normalizedLocale);
        var csvHash = Sha256(csv);
        var generatedAt = timeProvider.GetUtcNow();
        var manifest = BuildManifest(contractId, dataSource, normalizedLocale, filter, generatedAt, page, csvHash);
        var manifestHash = Sha256(manifest);
        var checksums = Encoding.UTF8.GetBytes($"{csvHash}  audit.csv\n{manifestHash}  manifest.json\n");

        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "audit.csv", csv);
            WriteEntry(archive, "manifest.json", manifest);
            WriteEntry(archive, "checksums.sha256", checksums);
        }

        var content = output.ToArray();
        return new AuditExportPackage(
            content,
            $"contract-audit-{contractId:N}-{generatedAt:yyyyMMddHHmmss}.zip",
            Sha256(content));
    }

    private static byte[] BuildManifest(
        Guid contractId,
        ContractAuditDataSource dataSource,
        string locale,
        TimelineFilter filter,
        DateTimeOffset generatedAt,
        TimelinePage page,
        string csvHash)
    {
        var disclaimer = locale == "pl"
            ? "Eksport wspiera audyt, ale nie jest kwalifikowanym dowodem elektronicznym."
            : "This export supports audit work but is not qualified electronic evidence.";
        return JsonSerializer.SerializeToUtf8Bytes(
            new
            {
                schemaVersion = 1,
                generatedAt,
                contractId,
                dataSource = GetDataSourceName(dataSource),
                locale,
                filters = filter,
                page.SnapshotSequence,
                page.SynchronizedAt,
                eventCount = page.Items.Count,
                rowCount = page.Items.Sum(item => Math.Max(item.Changes.Count, 1)),
                csvSha256 = csvHash,
                disclaimer
            },
            ManifestJsonOptions);
    }

    private static byte[] CreateCsv(
        Guid contractId,
        IReadOnlyList<TimelineItem> items,
        string locale)
    {
        var headers = locale == "pl"
            ? new[]
            {
                "Id umowy", "Id zdarzenia", "Sekwencja", "Data UTC", "Id operacji",
                "Użytkownik", "Rodzaj zmiany", "Kod zmiany", "Typ obiektu", "Kod obiektu",
                "Pole", "Przed", "Po", "Ostrzeżenia jakości"
            }
            :
            [
                "Contract ID", "Event ID", "Sequence", "Occurred at UTC", "Operation ID",
                "Actor", "Change type", "Change code", "Entity type", "Entity code",
                "Field", "Before", "After", "Data-quality warnings"
            ];

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', headers.Select(EscapeCsv)));

        foreach (var item in items)
        {
            var changes = item.Changes.Count > 0
                ? item.Changes
                : [new TimelineFieldChange(string.Empty, null, null, "empty")];
            foreach (var change in changes)
            {
                var cells = new[]
                {
                    contractId.ToString("D"),
                    item.EventId.ToString("D"),
                    item.SourceSequence.ToString(CultureInfo.InvariantCulture),
                    item.OccurredAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
                    item.CorrelationId.ToString("D"),
                    item.Actor,
                    item.ChangeKind,
                    item.ChangeKindCode.ToString(CultureInfo.InvariantCulture),
                    item.EntityKind,
                    item.EntityKindCode.ToString(CultureInfo.InvariantCulture),
                    change.Field,
                    change.Before ?? string.Empty,
                    change.After ?? string.Empty,
                    string.Join(';', item.DataQualityIssues)
                };
                builder.AppendLine(string.Join(',', cells.Select(EscapeCsv)));
            }
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(builder.ToString());
    }

    private static string EscapeCsv(string value)
    {
        var safeValue = value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
            ? $"'{value}"
            : value;
        return $"\"{safeValue.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static void WriteEntry(ZipArchive archive, string name, byte[] content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.SmallestSize);
        using var stream = entry.Open();
        stream.Write(content);
    }

    private static string Sha256(byte[] content) =>
        Convert.ToHexStringLower(SHA256.HashData(content));

    private static string GetDataSourceName(ContractAuditDataSource dataSource) =>
        dataSource switch
        {
            ContractAuditDataSource.Active => "active",
            ContractAuditDataSource.Archive => "archive",
            _ => throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, null)
        };
}
