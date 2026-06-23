using System.Text.Json;
using Audit.Contracts;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Entities;
using Audit.Infrastructure.Persistence.Persisters;

namespace Audit.Infrastructure.Tests;

[Collection(SqlServerFixtureGroup.Name)]
public sealed class ContractSearchProjectionWriterTests
{
    private readonly Fixture fixture;

    private readonly SqlServerFixture db;

    private readonly Guid OrgId;
    private readonly Guid ContractId;

    public ContractSearchProjectionWriterTests(SqlServerFixture db)
    {
        fixture = new();
        this.db = db;

        OrgId = fixture.Create<Guid>();
        ContractId = fixture.Create<Guid>();
    }

    [Fact(Skip = "Run requires Docker")]
    public async Task WhenUpdatingContractGivenFirstEventThenCreatesProjection()
    {
        await using var context = db.CreateContext();
        var writer = new ContractSearchProjectionWriter(context);
        var message = CreateContractMessage(sequence: 1, number: "UM/001");

        await writer.UpdateContractAsync(OrgId, ContractId, message, CancellationToken.None);

        await using var readCtx = db.CreateContext();
        var contract = await readCtx.Contracts.FindAsync(OrgId, ContractId);
        contract.Should().NotBeNull();
        contract!.Number.Should().Be("UM/001");
        contract.InternalNumber.Should().BeNull();
        contract.Subject.Should().BeNull();
        contract.ContractorName.Should().BeNull();
        contract.LastSourceSequence.Should().Be(1);
    }

    [Fact(Skip = "Run requires Docker")]
    public async Task WhenUpdatingContractGivenHigherSequenceThenUpdatesProjection()
    {
        await using var context = db.CreateContext();
        var writer = new ContractSearchProjectionWriter(context);
        var org = fixture.Create<Guid>();
        var contract = fixture.Create<Guid>();

        await writer.UpdateContractAsync(org, contract, CreateContractMessage(1, "UM/OLD"), CancellationToken.None);
        await writer.UpdateContractAsync(org, contract, CreateContractMessage(2, "UM/NEW"), CancellationToken.None);

        await using var readCtx = db.CreateContext();
        var projection = await readCtx.Contracts.FindAsync(org, contract);
        projection.Should().NotBeNull();
        projection!.Number.Should().Be("UM/NEW");
        projection.LastSourceSequence.Should().Be(2);
    }

    [Fact(Skip = "Run requires Docker")]
    public async Task WhenUpdatingContractGivenStringNullFieldThenNormalizesToNull()
    {
        await using var context = db.CreateContext();
        var writer = new ContractSearchProjectionWriter(context);

        var after = JsonDocument.Parse(
            """{"Number":"null","InternalNumber":" ","Subject":"null","ContractorName":"Acme"}""");
        var message = new AuditEntryImportedV1(
            EventId: fixture.Create<Guid>(),
            SchemaVersion: 1,
            Source: "contracts-sql",
            SourceEventId: 1,
            SourceSequence: 1,
            OrganizationId: OrgId,
            OccurredAt: fixture.Create<DateTimeOffset>(),
            IngestedAt: fixture.Create<DateTimeOffset>(),
            Actor: new ActorSnapshotV1(null, "system"),
            Operation: new AuditOperationV1(fixture.Create<Guid>(), 1),
            Entity: new AuditedEntityV1(1, ContractId, null, null),
            Before: null,
            After: after.RootElement.Clone(),
            ChangedFields: ["Number", "InternalNumber", "Subject", "ContractorName"]);

        await writer.UpdateContractAsync(OrgId, ContractId, message, CancellationToken.None);

        await using var readCtx = db.CreateContext();
        var projection = await readCtx.Contracts.FindAsync(OrgId, ContractId);
        projection.Should().NotBeNull();
        projection!.Number.Should().BeNull();
        projection.InternalNumber.Should().BeNull();
        projection.Subject.Should().BeNull();
        projection.ContractorName.Should().Be("Acme");
    }

    [Fact(Skip = "Run requires Docker")]
    public async Task WhenUpdatingContractGivenLowerSequenceThenDoesNotDowngradeProjection()
    {
        await using var context = db.CreateContext();
        var writer = new ContractSearchProjectionWriter(context);
        var org = fixture.Create<Guid>();
        var contract = fixture.Create<Guid>();

        await writer.UpdateContractAsync(org, contract, CreateContractMessage(5, "UM/CURRENT"), CancellationToken.None);
        await writer.UpdateContractAsync(org, contract, CreateContractMessage(3, "UM/STALE"), CancellationToken.None);

        await using var readCtx = db.CreateContext();
        var projection = await readCtx.Contracts.FindAsync(org, contract);
        projection.Should().NotBeNull();
        projection!.Number.Should().Be("UM/CURRENT");
        projection.LastSourceSequence.Should().Be(5);
    }

    [Fact(Skip = "Run requires Docker")]
    public async Task WhenTrackingActivityGivenContractIsArchivedThenSetsReactivationPending()
    {
        await using var setup = db.CreateContext();
        var org = fixture.Create<Guid>();
        var contract = fixture.Create<Guid>();

        setup.Contracts.Add(new ContractSearchEntity
        {
            OrganizationId = org,
            ContractId = contract,
            LastSourceSequence = 1,
            LastActivityAt = DateTimeOffset.UtcNow.AddYears(-6)
        });
        setup.ContractArchiveTransfers.Add(new ContractArchiveTransferEntity
        {
            OrganizationId = org,
            ContractId = contract,
            State = "Archived",
            StartedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await setup.SaveChangesAsync();

        await using var context = db.CreateContext();
        var writer = new ContractSearchProjectionWriter(context);
        await writer.TrackActivityAsync(org, contract, DateTimeOffset.UtcNow, CancellationToken.None);

        await using var readCtx = db.CreateContext();
        var transfer = await readCtx.ContractArchiveTransfers.FindAsync(org, contract);
        transfer.Should().NotBeNull();
        transfer!.State.Should().Be("ReactivationPending");
    }

    private AuditEntryImportedV1 CreateContractMessage(long sequence, string number)
    {
        var after = JsonDocument.Parse(
            $"{{\"Number\":\"{number}\",\"InternalNumber\":null,\"Subject\":null,\"ContractorName\":null}}");
        return new AuditEntryImportedV1(
            EventId: fixture.Create<Guid>(),
            SchemaVersion: 1,
            Source: "contracts-sql",
            SourceEventId: sequence,
            SourceSequence: sequence,
            OrganizationId: OrgId,
            OccurredAt: fixture.Create<DateTimeOffset>(),
            IngestedAt: fixture.Create<DateTimeOffset>(),
            Actor: new ActorSnapshotV1(null, "system"),
            Operation: new AuditOperationV1(fixture.Create<Guid>(), 1),
            Entity: new AuditedEntityV1(1, ContractId, null, null),
            Before: null,
            After: after.RootElement.Clone(),
            ChangedFields: ["Number"]);
    }
}
