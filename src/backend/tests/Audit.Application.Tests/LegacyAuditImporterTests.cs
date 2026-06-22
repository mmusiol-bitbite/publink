using Audit.Application.Legacy;
using Audit.Contracts;

namespace Audit.Application.Tests;

public sealed class LegacyAuditImporterTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
    private readonly Fixture fixture = new();

    [Fact]
    public async Task WhenImportingBatchGivenAllPublicationsSucceedThenPublishesAndAdvancesCheckpoint()
    {
        var calls = new List<string>();
        var checkpoint = new RecordingCheckpointStore(10, calls);
        var publisher = new RecordingPublisher(calls);
        var importer = CreateImporter(
            new StubReader([CreateRecord(11), CreateRecord(12)]),
            checkpoint,
            publisher);

        var result = await importer.ImportNextBatchAsync(CancellationToken.None);

        result.PublishedCount.Should().Be(2);
        result.LastSourceEventId.Should().Be(12);
        calls.Should().BeEquivalentTo(
            ["publish:11", "checkpoint:11", "publish:12", "checkpoint:12"],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task WhenImportingBatchGivenPublicationFailsThenDoesNotAdvancePastFailedEvent()
    {
        var calls = new List<string>();
        var checkpoint = new RecordingCheckpointStore(10, calls);
        var publisher = new RecordingPublisher(
            calls,
            failOnSourceEventId: 12,
            failureMessage: fixture.Create<string>());
        var importer = CreateImporter(
            new StubReader([CreateRecord(11), CreateRecord(12), CreateRecord(13)]),
            checkpoint,
            publisher);

        var act = () => importer.ImportNextBatchAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        checkpoint.Value.Should().Be(11);
        calls.Should().BeEquivalentTo(
            ["publish:11", "checkpoint:11", "publish:12"],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task WhenMarkingSweepCompletedGivenNoRecordsThenRecordsSuccessfulEmptySynchronization()
    {
        var calls = new List<string>();
        var checkpoint = new RecordingCheckpointStore(10, calls);
        var importer = CreateImporter(new StubReader([]), checkpoint, new RecordingPublisher(calls));

        await importer.MarkSweepCompletedAsync(CancellationToken.None);

        checkpoint.LastSynchronizedAt.Should().Be(Now);
        calls.Should().BeEquivalentTo(["synchronized"]);
    }

    private static LegacyAuditImporter CreateImporter(
        ILegacyAuditReader reader,
        IImportCheckpointStore checkpoint,
        IAuditEventPublisher publisher)
    {
        var options = new LegacyImportOptions
        {
            Source = "contracts-sql",
            SourceTimeZone = "UTC",
            BatchSize = 100
        };
        var time = new FixedTimeProvider(Now);
        return new LegacyAuditImporter(
            reader,
            checkpoint,
            publisher,
            new LegacyAuditEventMapper(time, options),
            time,
            options);
    }

    private static LegacyAuditRecord CreateRecord(int id) =>
        new(
            id,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            null,
            null,
            1,
            1,
            new DateTime(2026, 6, 18, 8, 15, 0, DateTimeKind.Unspecified),
            null,
            "{}",
            "[]",
            id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            null,
            Guid.Parse("33333333-3333-3333-3333-333333333333"));

    private sealed class StubReader(IReadOnlyList<LegacyAuditRecord> records) : ILegacyAuditReader
    {
        public Task<IReadOnlyList<LegacyAuditRecord>> ReadAfterAsync(
            long sourceEventId,
            int batchSize,
            CancellationToken cancellationToken)
        {
            sourceEventId.Should().Be(10);
            batchSize.Should().Be(100);
            return Task.FromResult(records);
        }
    }

    private sealed class RecordingCheckpointStore(long value, List<string> calls)
        : IImportCheckpointStore
    {
        public long Value { get; private set; } = value;

        public DateTimeOffset? LastSynchronizedAt { get; private set; }

        public Task<long> GetAsync(string source, CancellationToken cancellationToken) =>
            Task.FromResult(Value);

        public Task SaveAsync(
            string source,
            long sourceEventId,
            DateTimeOffset updatedAt,
            CancellationToken cancellationToken)
        {
            Value = sourceEventId;
            calls.Add($"checkpoint:{sourceEventId}");
            return Task.CompletedTask;
        }

        public Task MarkSynchronizedAsync(
            string source,
            DateTimeOffset synchronizedAt,
            CancellationToken cancellationToken)
        {
            LastSynchronizedAt = synchronizedAt;
            calls.Add("synchronized");
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPublisher(
        List<string> calls,
        long? failOnSourceEventId = null,
        string failureMessage = "simulatedBrokerFailure")
        : IAuditEventPublisher
    {
        public Task PublishAsync(
            AuditEntryImportedV1 message,
            CancellationToken cancellationToken)
        {
            calls.Add($"publish:{message.SourceEventId}");
            return message.SourceEventId == failOnSourceEventId
                ? Task.FromException(new InvalidOperationException(failureMessage))
                : Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
