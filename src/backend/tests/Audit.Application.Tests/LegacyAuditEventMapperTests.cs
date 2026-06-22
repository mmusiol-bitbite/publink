using Audit.Application.Legacy;

namespace Audit.Application.Tests;

public sealed class LegacyAuditEventMapperTests
{
    private readonly Fixture fixture;
    private readonly DateTimeOffset IngestedAt;

    public LegacyAuditEventMapperTests()
    {
        fixture = new();
        IngestedAt = fixture.Create<DateTimeOffset>();
    }

    [Fact]
    public void WhenMappingLegacyRecordGivenSourceCodesThenPreservesCodesAndCreatesDeterministicIdentity()
    {
        var mapper = CreateMapper();

        var recordId= fixture.Create<int>();
        var record = CreateRecord(recordId);

        var first = mapper.Map(record);
        var replay = mapper.Map(record);

        replay.EventId.Should().Be(first.EventId);
        first.SourceEventId.Should().Be(recordId);
        first.Entity.EntityTypeCode.Should().Be(14);
        first.Operation.ChangeTypeCode.Should().Be(99);
        first.IngestedAt.Should().Be(IngestedAt);
        first.OccurredAt.Offset.Should().Be(TimeSpan.Zero);
        first.Before?.GetProperty("Status").GetString().Should().Be("before");
        first.ChangedFields.Should().BeEquivalentTo(["Status"]);
    }

    [Fact]
    public void WhenMappingLegacyRecordGivenMalformedJsonThenTreatsPayloadAsMissingWithoutStoppingImport()
    {
        var mapper = CreateMapper();
        var record = CreateRecord(fixture.Create<int>()) with
        {
            OldValues = "not-json",
            AffectedColumns = "{also-not-an-array:true}"
        };

        var message = mapper.Map(record);

        message.Before.Should().BeNull();
        message.ChangedFields.Should().BeEmpty();
        message.After?.GetProperty("Status").GetString().Should().Be("after");
    }

    private LegacyAuditEventMapper CreateMapper() =>
        new(
            new FixedTimeProvider(IngestedAt),
            new LegacyImportOptions { Source = "contracts-sql", SourceTimeZone = "UTC" });

    private LegacyAuditRecord CreateRecord(int id) =>
        new(
            id,
            fixture.Create<Guid>(),
            fixture.Create<Guid>(),
            "user@example.gov.pl",
            99,
            14,
            fixture.Create<DateTime>(),
            "{\"Status\":\"before\"}",
            "{\"Status\":\"after\"}",
            "[\"Status\"]",
            "42",
            fixture.Create<Guid>(),
            null,
            fixture.Create<Guid>());

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
