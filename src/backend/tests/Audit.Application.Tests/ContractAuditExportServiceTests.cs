using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Audit.Application.Exports;
using Audit.Application.Queries;

namespace Audit.Application.Tests;

public sealed class ContractAuditExportServiceTests
{
    private readonly Fixture fixture;

    private readonly Guid ContractId;
    private readonly DateTimeOffset GeneratedAt;

    public ContractAuditExportServiceTests()
    {
        fixture = new();
        ContractId = fixture.Create<Guid>();
        GeneratedAt = fixture.Create<DateTimeOffset>();
    }

    [Fact]
    public async Task WhenGeneratingExportGivenFormulaLikeValuesThenCreatesVerifiableZipAndProtectsCsv()
    {
        var reader = new StubTimelineReader(CreatePage(nextCursor: null));
        var service = new ContractAuditExportService(new FixedTimeProvider(GeneratedAt));

        var package = await service.GenerateAsync(
            reader,
            ContractAuditDataSource.Active,
            ContractId,
            new TimelineFilter(null, null, null, null, null),
            "pl",
            CancellationToken.None);

        package.Sha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(package.Content.Span)));
        using var archive = new ZipArchive(
            new MemoryStream(package.Content.ToArray()),
            ZipArchiveMode.Read);
        archive.Entries.Should().HaveCount(3);

        var csv = await ReadEntryAsync(archive, "audit.csv");
        csv.Should().Contain("'=@dangerous");
        csv.Should().Contain(ContractId.ToString("D"));

        var manifest = JsonDocument.Parse(await ReadEntryAsync(archive, "manifest.json"));
        manifest.RootElement.GetProperty("eventCount").GetInt32().Should().Be(1);
        manifest.RootElement.GetProperty("locale").GetString().Should().Be("pl");
        manifest.RootElement.GetProperty("dataSource").GetString().Should().Be("active");

        var checksums = await ReadEntryAsync(archive, "checksums.sha256");
        checksums.Should().Contain("audit.csv");
        checksums.Should().Contain("manifest.json");
    }

    [Fact]
    public async Task WhenGeneratingExportGivenArchiveSourceThenMarksManifestDataSourceAsArchive()
    {
        var reader = new StubTimelineReader(CreatePage(nextCursor: null));
        var service = new ContractAuditExportService(new FixedTimeProvider(GeneratedAt));

        var package = await service.GenerateAsync(
            reader,
            ContractAuditDataSource.Archive,
            ContractId,
            new TimelineFilter(null, null, null, null, null),
            "en",
            CancellationToken.None);

        using var archive = new ZipArchive(
            new MemoryStream(package.Content.ToArray()),
            ZipArchiveMode.Read);
        var manifest = JsonDocument.Parse(await ReadEntryAsync(archive, "manifest.json"));
        manifest.RootElement.GetProperty("dataSource").GetString().Should().Be("archive");
    }

    [Fact]
    public async Task WhenGeneratingExportGivenSnapshotExceedsLimitThenRejectsExport()
    {
        var reader = new StubTimelineReader(CreatePage(nextCursor: "more"));
        var service = new ContractAuditExportService(new FixedTimeProvider(GeneratedAt));

        var act = () => service.GenerateAsync(
            reader,
            ContractAuditDataSource.Active,
            ContractId,
            new TimelineFilter(null, null, null, null, null),
            "en",
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ExportTooLargeException>();
        exception.Which.MaximumEvents.Should().Be(10_000);
    }

    private TimelinePage CreatePage(string? nextCursor) => new(
        ContractId,
        17,
        GeneratedAt.AddMinutes(-5),
        [
            new TimelineItem(
                fixture.Create<Guid>(),
                17,
                GeneratedAt.AddDays(-1),
                fixture.Create<Guid>(),
                "modified",
                3,
                "contractHeader",
                1,
                "auditor@example.gov.pl",
                [new TimelineFieldChange("Subject", "=@dangerous", "Safe", "string")],
                [])
        ],
        nextCursor);

    private static async Task<string> ReadEntryAsync(ZipArchive archive, string name)
    {
        var entry = archive.Entries.Should().ContainSingle(item => item.FullName == name).Which;
        await using var stream = await entry.OpenAsync(CancellationToken.None);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubTimelineReader(TimelinePage page) : IContractTimelineSource
    {
        public Task<TimelinePage> ReadAsync(
            Guid contractId,
            int limit,
            string? cursor,
            TimelineFilter filter,
            CancellationToken cancellationToken) => Task.FromResult(page);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
