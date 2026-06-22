using Audit.Application.Archiving;
using Audit.Infrastructure.Archiving.Execution;
using Audit.Infrastructure.Archiving.Lifecycle;
using Audit.Infrastructure.Archiving.Reactivation;
using Audit.Infrastructure.Archiving.Snapshots;
using Audit.Infrastructure.Archiving.Transfers;
using Audit.Infrastructure.Persistence.Contexts;
using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Tests;

[Collection(SqlServerFixtureGroup.Name)]
public sealed class DatabaseContractArchiveLifecycleTests(SqlServerFixture db)
{
    private readonly Fixture fixture = new();
    private static int sourceEventIdSeed = 10_000;

    [Fact]
    public async Task WhenRunningLifecycleGivenPendingReactivationSnapshotThenRestoresSnapshot()
    {
        await using var activeContext = db.CreateContext();
        await using var archiveContext = db.CreateArchiveContext();
        await ResetArchivalTablesAsync(activeContext, archiveContext);
        var snapshot = CreateArchivedSnapshot(fixture);
        await AddArchivedSnapshotAsync(archiveContext, snapshot);
        activeContext.ContractArchiveTransfers.Add(CreateTransfer(
            snapshot.Contract.OrganizationId,
            snapshot.Contract.ContractId,
            "ReactivationPending",
            snapshot.Contract.LastActivityAt,
            snapshot.Timeline.Count,
            snapshot.Timeline.Max(item => item.SourceSequence),
            snapshot.Events.Count));
        await activeContext.SaveChangesAsync();

        var lifecycle = CreateLifecycle(activeContext, archiveContext);

        var result = await lifecycle.RunOnceAsync(CancellationToken.None);

        result.Processed.Should().BeTrue();
        result.Action.Should().Be("Reactivated");
        result.EventCount.Should().Be(snapshot.Events.Count);

        await using var activeReadContext = db.CreateContext();
        await using var archiveReadContext = db.CreateArchiveContext();
        (await activeReadContext.Contracts.FindAsync(snapshot.Contract.OrganizationId, snapshot.Contract.ContractId)).Should().NotBeNull();
        (await activeReadContext.AuditEvents.FindAsync(snapshot.Events.First().EventId)).Should().NotBeNull();
        (await activeReadContext.TimelineItems.FindAsync(snapshot.Timeline.First().EventId)).Should().NotBeNull();
        (await archiveReadContext.Contracts.FindAsync(snapshot.Contract.OrganizationId, snapshot.Contract.ContractId)).Should().BeNull();

        var transfer = await activeReadContext.ContractArchiveTransfers.FindAsync(snapshot.Contract.OrganizationId, snapshot.Contract.ContractId);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("Active");
        transfer.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task WhenRunningLifecycleGivenPendingReactivationWithoutSnapshotThenRecoversTransfer()
    {
        await using var activeContext = db.CreateContext();
        await using var archiveContext = db.CreateArchiveContext();
        await ResetArchivalTablesAsync(activeContext, archiveContext);
        var organizationId = fixture.Create<Guid>();
        var contractId = fixture.Create<Guid>();
        activeContext.ContractArchiveTransfers.Add(CreateTransfer(
            organizationId,
            contractId,
            "ReactivationPending",
            DateTimeOffset.UtcNow.AddYears(-6),
            timelineCount: 1,
            snapshotSequence: 1,
            eventCount: 1));
        await activeContext.SaveChangesAsync();

        var lifecycle = CreateLifecycle(activeContext, archiveContext);

        var result = await lifecycle.RunOnceAsync(CancellationToken.None);

        result.Processed.Should().BeTrue();
        result.Action.Should().Be("ReactivationRecovered");
        result.EventCount.Should().Be(0);

        await using var activeReadContext = db.CreateContext();
        var transfer = await activeReadContext.ContractArchiveTransfers.FindAsync(organizationId, contractId);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("Active");
        transfer.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task WhenRunningLifecycleGivenArchivalCopyFailsAfterTransferStartedThenMarksTransferFailed()
    {
        await using var activeContext = db.CreateContext();
        await using var archiveContext = db.CreateArchiveContext();
        await ResetArchivalTablesAsync(activeContext, archiveContext);
        var organizationId = fixture.Create<Guid>();
        var contractId = fixture.Create<Guid>();
        var eventId = fixture.Create<Guid>();
        var sourceEventId = NextSourceEventId();
        var lastActivityAt = DateTimeOffset.UtcNow.AddYears(-6);
        await AddActiveContractAsync(
            activeContext,
            fixture,
            organizationId,
            contractId,
            eventId,
            sourceEventId,
            lastActivityAt);
        archiveContext.AuditEvents.Add(new ArchivedAuditEventEntity
        {
            EventId = fixture.Create<Guid>(),
            OrganizationId = fixture.Create<Guid>(),
            ContractId = fixture.Create<Guid>(),
            Source = "contracts-sql",
            SourceEventId = sourceEventId,
            SourceSequence = sourceEventId,
            OccurredAt = lastActivityAt,
            IngestedAt = lastActivityAt,
            CorrelationId = fixture.Create<Guid>(),
            ChangeTypeCode = 1,
            EntityTypeCode = 1,
            ChangedFieldsJson = "[]"
        });
        await archiveContext.SaveChangesAsync();

        var lifecycle = CreateLifecycle(activeContext, archiveContext);

        var act = () => lifecycle.RunOnceAsync(CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();

        await using var activeReadContext = db.CreateContext();
        var transfer = await activeReadContext.ContractArchiveTransfers.FindAsync(organizationId, contractId);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("Failed");
        transfer.ErrorCode.Should().Be(nameof(DbUpdateException));
    }

    private static DatabaseContractArchiveLifecycle CreateLifecycle(
        AuditDbContext activeContext,
        ArchiveDbContext archiveContext)
    {
        var snapshotStore = new ContractArchiveSnapshotStore(archiveContext);
        var transferStore = new ContractArchiveTransferStore(activeContext, TimeProvider.System);
        return new DatabaseContractArchiveLifecycle(
            activeContext,
            new ContractArchivalExecutor(activeContext, snapshotStore, TimeProvider.System),
            new ArchivedContractReactivator(
                snapshotStore,
                new ArchivedContractRestorer(activeContext),
                transferStore),
            transferStore,
            TimeProvider.System,
            new ContractArchivalOptions { InactivityMonths = 18 });
    }

    private static async Task ResetArchivalTablesAsync(
        AuditDbContext activeContext,
        ArchiveDbContext archiveContext)
    {
        await activeContext.ContractArchiveTransfers.ExecuteDeleteAsync();
        await activeContext.TimelineItems.ExecuteDeleteAsync();
        await activeContext.ContractAliases.ExecuteDeleteAsync();
        await activeContext.AuditEvents.ExecuteDeleteAsync();
        await activeContext.Contracts.ExecuteDeleteAsync();

        await archiveContext.TimelineItems.ExecuteDeleteAsync();
        await archiveContext.ContractAliases.ExecuteDeleteAsync();
        await archiveContext.AuditEvents.ExecuteDeleteAsync();
        await archiveContext.Contracts.ExecuteDeleteAsync();
    }

    private static async Task AddActiveContractAsync(
        AuditDbContext context,
        Fixture fixture,
        Guid organizationId,
        Guid contractId,
        Guid eventId,
        long sourceEventId,
        DateTimeOffset lastActivityAt)
    {
        context.Contracts.Add(new ContractSearchEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Number = $"ARCH/{sourceEventId}",
            LastSourceSequence = sourceEventId,
            LastActivityAt = lastActivityAt
        });
        context.AuditEvents.Add(new CanonicalAuditEventEntity
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
        context.TimelineItems.Add(new ContractTimelineItemEntity
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
        await context.SaveChangesAsync();
    }

    private static ArchivedContractSnapshot CreateArchivedSnapshot(Fixture fixture)
    {
        var organizationId = fixture.Create<Guid>();
        var contractId = fixture.Create<Guid>();
        var eventId = fixture.Create<Guid>();
        var sourceEventId = NextSourceEventId();
        var lastActivityAt = DateTimeOffset.UtcNow.AddYears(-6);
        var contract = new ArchivedContractEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Number = $"ARCH/{sourceEventId}",
            LastSourceSequence = sourceEventId,
            LastActivityAt = lastActivityAt,
            ArchivedAt = DateTimeOffset.UtcNow
        };
        var aliases = new List<ArchivedContractAliasEntity>
        {
            new()
            {
                OrganizationId = organizationId,
                ContractId = contractId,
                Field = "Number",
                Value = contract.Number,
                IsCurrent = true
            }
        };
        var events = new List<ArchivedAuditEventEntity>
        {
            new()
            {
                EventId = eventId,
                OrganizationId = organizationId,
                ContractId = contractId,
                Source = "contracts-sql",
                SourceEventId = sourceEventId,
                SourceSequence = sourceEventId,
                OccurredAt = lastActivityAt,
                IngestedAt = lastActivityAt,
                CorrelationId = fixture.Create<Guid>(),
                ChangeTypeCode = 1,
                EntityTypeCode = 1,
                EntityId = contractId,
                ChangedFieldsJson = "[]"
            }
        };
        var timeline = new List<ArchivedTimelineItemEntity>
        {
            new()
            {
                EventId = eventId,
                OrganizationId = organizationId,
                ContractId = contractId,
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
            }
        };

        return new ArchivedContractSnapshot(contract, aliases, events, timeline);
    }

    private static async Task AddArchivedSnapshotAsync(
        ArchiveDbContext context,
        ArchivedContractSnapshot snapshot)
    {
        context.Contracts.Add(snapshot.Contract);
        context.ContractAliases.AddRange(snapshot.Aliases);
        context.AuditEvents.AddRange(snapshot.Events);
        context.TimelineItems.AddRange(snapshot.Timeline);
        await context.SaveChangesAsync();
    }

    private static ContractArchiveTransferEntity CreateTransfer(
        Guid organizationId,
        Guid contractId,
        string state,
        DateTimeOffset lastActivityAt,
        int timelineCount,
        long snapshotSequence,
        int eventCount)
    {
        var now = DateTimeOffset.UtcNow;
        return new ContractArchiveTransferEntity
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            State = state,
            LastActivityAt = lastActivityAt,
            TimelineCount = timelineCount,
            SnapshotSequence = snapshotSequence,
            EventCount = eventCount,
            StartedAt = now,
            UpdatedAt = now
        };
    }

    private static int NextSourceEventId() =>
        System.Threading.Interlocked.Increment(ref sourceEventIdSeed);
}
