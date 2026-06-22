using Audit.Infrastructure.Archiving.Execution;
using Audit.Infrastructure.Archiving.Snapshots;
using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Tests;

[Collection(SqlServerFixtureGroup.Name)]
public sealed class ContractArchivalExecutorTests(SqlServerFixture db)
{
    private static int sourceEventIdSeed = 1_000;
    private readonly Fixture fixture = new();

    [Fact]
    public async Task WhenExecutingArchivalGivenStableCandidateThenMovesContractSnapshotToArchive()
    {
        await using var activeContext = db.CreateContext();
        await using var archiveContext = db.CreateArchiveContext();
        var organizationId = fixture.Create<Guid>();
        var contractId = fixture.Create<Guid>();
        var eventId = fixture.Create<Guid>();
        var sourceEventId = System.Threading.Interlocked.Increment(ref sourceEventIdSeed);
        var lastActivityAt = DateTimeOffset.UtcNow.AddYears(-6);

        var contract = new ContractSearchEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Number = "ARCH/001",
            LastSourceSequence = 10,
            LastActivityAt = lastActivityAt
        };
        activeContext.Contracts.Add(contract);
        activeContext.ContractAliases.Add(new ContractSearchAliasEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Field = "Number",
            Value = "ARCH/001",
            IsCurrent = true
        });
        activeContext.AuditEvents.Add(new CanonicalAuditEventEntity
        {
            EventId = eventId,
            Source = "contracts-sql",
            SourceEventId = sourceEventId,
            SourceSequence = sourceEventId,
            OrganizationId = organizationId,
            OccurredAt = lastActivityAt,
            IngestedAt = lastActivityAt,
            CorrelationId = fixture.Create<Guid>(),
            ChangeTypeCode = 1,
            EntityTypeCode = 1,
            EntityId = contractId,
            ChangedFieldsJson = "[]"
        });
        activeContext.TimelineItems.Add(new ContractTimelineItemEntity
        {
            EventId = eventId,
            OrganizationId = organizationId,
            ContractId = contractId,
            ContractIdResolved = true,
            SourceSequence = sourceEventId,
            OccurredAt = lastActivityAt,
            CorrelationId = fixture.Create<Guid>(),
            ChangeTypeCode = 1,
            ChangeKind = "Updated",
            EntityTypeCode = 1,
            EntityKind = "Contract",
            Actor = "system",
            ChangesJson = "[]",
            DataQualityIssuesJson = "[]"
        });
        await activeContext.SaveChangesAsync();

        var executor = new ContractArchivalExecutor(
            activeContext,
            new ContractArchiveSnapshotStore(archiveContext),
            TimeProvider.System);

        var result = await executor.ExecuteAsync(contract, CancellationToken.None);

        result.Processed.Should().BeTrue();
        result.Action.Should().Be("Archived");
        result.EventCount.Should().Be(1);

        await using var activeReadContext = db.CreateContext();
        await using var archiveReadContext = db.CreateArchiveContext();
        (await activeReadContext.Contracts.FindAsync(organizationId, contractId)).Should().BeNull();
        (await archiveReadContext.Contracts.FindAsync(organizationId, contractId)).Should().NotBeNull();
        (await archiveReadContext.AuditEvents.FindAsync(eventId)).Should().NotBeNull();

        var transfer = await activeReadContext.ContractArchiveTransfers.FindAsync(organizationId, contractId);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("Archived");
        transfer.EventCount.Should().Be(1);
        transfer.TimelineCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenExecutingArchivalGivenCandidateChangedAfterSelectionThenCancelsCopy()
    {
        await using var activeContext = db.CreateContext();
        await using var archiveContext = db.CreateArchiveContext();
        var organizationId = fixture.Create<Guid>();
        var contractId = fixture.Create<Guid>();
        var eventId = fixture.Create<Guid>();
        var sourceEventId = System.Threading.Interlocked.Increment(ref sourceEventIdSeed);
        var selectedLastActivityAt = DateTimeOffset.UtcNow.AddYears(-6);
        var currentLastActivityAt = selectedLastActivityAt.AddMinutes(1);

        activeContext.Contracts.Add(new ContractSearchEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Number = "ARCH/CHANGED",
            LastSourceSequence = 10,
            LastActivityAt = currentLastActivityAt
        });
        activeContext.AuditEvents.Add(new CanonicalAuditEventEntity
        {
            EventId = eventId,
            Source = "contracts-sql",
            SourceEventId = sourceEventId,
            SourceSequence = sourceEventId,
            OrganizationId = organizationId,
            OccurredAt = selectedLastActivityAt,
            IngestedAt = selectedLastActivityAt,
            CorrelationId = fixture.Create<Guid>(),
            ChangeTypeCode = 1,
            EntityTypeCode = 1,
            EntityId = contractId,
            ChangedFieldsJson = "[]"
        });
        activeContext.TimelineItems.Add(new ContractTimelineItemEntity
        {
            EventId = eventId,
            OrganizationId = organizationId,
            ContractId = contractId,
            ContractIdResolved = true,
            SourceSequence = sourceEventId,
            OccurredAt = selectedLastActivityAt,
            CorrelationId = fixture.Create<Guid>(),
            ChangeTypeCode = 1,
            ChangeKind = "Updated",
            EntityTypeCode = 1,
            EntityKind = "Contract",
            Actor = "system",
            ChangesJson = "[]",
            DataQualityIssuesJson = "[]"
        });
        await activeContext.SaveChangesAsync();

        var selectedCandidate = new ContractSearchEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Number = "ARCH/CHANGED",
            LastSourceSequence = 10,
            LastActivityAt = selectedLastActivityAt
        };
        var executor = new ContractArchivalExecutor(
            activeContext,
            new ContractArchiveSnapshotStore(archiveContext),
            TimeProvider.System);

        var result = await executor.ExecuteAsync(selectedCandidate, CancellationToken.None);

        result.Processed.Should().BeTrue();
        result.Action.Should().Be("CopyCancelledBecauseContractChanged");
        result.EventCount.Should().Be(1);

        await using var activeReadContext = db.CreateContext();
        await using var archiveReadContext = db.CreateArchiveContext();
        (await activeReadContext.Contracts.FindAsync(organizationId, contractId)).Should().NotBeNull();
        (await archiveReadContext.Contracts.FindAsync(organizationId, contractId)).Should().BeNull();

        var transfer = await activeReadContext.ContractArchiveTransfers.FindAsync(organizationId, contractId);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("Active");
        transfer.ErrorCode.Should().BeNull();
    }
}
