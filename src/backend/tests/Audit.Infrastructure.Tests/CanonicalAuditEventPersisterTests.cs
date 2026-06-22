using Audit.Application.Persistence;
using Audit.Contracts;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Persisters;

namespace Audit.Infrastructure.Tests;

[Collection(SqlServerFixtureGroup.Name)]
public sealed class CanonicalAuditEventPersisterTests(SqlServerFixture db)
{
    private readonly Fixture fixture = new();

    [Fact]
    public async Task WhenAppendingAuditEventGivenFirstInsertThenReturnsAppended()
    {
        await using var context = db.CreateContext();
        var persister = new CanonicalAuditEventPersister(context);
        var message = CreateMessage(sourceEventId: 1);

        var result = await persister.TryAppendAsync(message, CancellationToken.None);
        await context.SaveChangesAsync();

        result.Should().Be(AppendAuditEventResult.Appended);
        context.AuditEvents.Should().ContainSingle(e => e.EventId == message.EventId);
    }

    [Fact]
    public async Task WhenAppendingAuditEventGivenSameSourceEventIdThenReturnsDuplicate()
    {
        await using var context = db.CreateContext();
        var persister = new CanonicalAuditEventPersister(context);
        var message = CreateMessage(sourceEventId: 2);

        await persister.TryAppendAsync(message, CancellationToken.None);
        await context.SaveChangesAsync();

        await using var context2 = db.CreateContext();
        var persister2 = new CanonicalAuditEventPersister(context2);

        var duplicate = await persister2.TryAppendAsync(message, CancellationToken.None);

        duplicate.Should().Be(AppendAuditEventResult.Duplicate);
    }

    [Fact]
    public async Task WhenAppendingAuditEventGivenDifferentSourceEventIdsThenAllowsAllEvents()
    {
        await using var context = db.CreateContext();
        var persister = new CanonicalAuditEventPersister(context);
        var first = CreateMessage(sourceEventId: 3);
        var second = CreateMessage(sourceEventId: 4);

        var firstResult = await persister.TryAppendAsync(first, CancellationToken.None);
        var secondResult = await persister.TryAppendAsync(second, CancellationToken.None);

        firstResult.Should().Be(AppendAuditEventResult.Appended);
        secondResult.Should().Be(AppendAuditEventResult.Appended);
    }

    private AuditEntryImportedV1 CreateMessage(long sourceEventId) => new(
        EventId: fixture.Create<Guid>(),
        SchemaVersion: 1,
        Source: "contracts-sql",
        SourceEventId: sourceEventId,
        SourceSequence: sourceEventId,
        OrganizationId: fixture.Create<Guid>(),
        OccurredAt: fixture.Create<DateTimeOffset>(),
        IngestedAt: fixture.Create<DateTimeOffset>(),
        Actor: new ActorSnapshotV1(fixture.Create<Guid>(), "user@example.com"),
        Operation: new AuditOperationV1(fixture.Create<Guid>(), 3),
        Entity: new AuditedEntityV1(1, fixture.Create<Guid>(), null, null),
        Before: null,
        After: null,
        ChangedFields: []);
}
