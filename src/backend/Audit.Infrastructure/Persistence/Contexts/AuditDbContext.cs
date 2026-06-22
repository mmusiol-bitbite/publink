using Audit.Application.Persistence;
using Audit.Infrastructure.Persistence.Entities;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Contexts;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
    : DbContext(options), IAuditUnitOfWork
{
    public DbSet<CanonicalAuditEventEntity> AuditEvents => Set<CanonicalAuditEventEntity>();

    public DbSet<ContractTimelineItemEntity> TimelineItems => Set<ContractTimelineItemEntity>();

    public DbSet<ContractSearchEntity> Contracts => Set<ContractSearchEntity>();

    public DbSet<ContractSearchAliasEntity> ContractAliases => Set<ContractSearchAliasEntity>();

    public DbSet<ImportCheckpointEntity> ImportCheckpoints => Set<ImportCheckpointEntity>();

    public DbSet<LegacySynchronizationRequestEntity> LegacySynchronizationRequests =>
        Set<LegacySynchronizationRequestEntity>();

    public DbSet<ContractArchiveTransferEntity> ContractArchiveTransfers =>
        Set<ContractArchiveTransferEntity>();

    public DbSet<InboxState> InboxStates => Set<InboxState>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<OutboxState> OutboxStates => Set<OutboxState>();

    public Task CommitAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<CanonicalAuditEventEntity>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasKey(item => item.EventId);
            entity.HasIndex(item => new { item.Source, item.SourceEventId }).IsUnique();
            entity.HasIndex(item => new { item.OrganizationId, item.SourceSequence });
            entity.Property(item => item.Source).HasSourceMaxLength();
            entity.Property(item => item.ActorEmail).HasActorEmailMaxLength();
            entity.Property(item => item.BeforeJson).HasJsonColumnType();
            entity.Property(item => item.AfterJson).HasJsonColumnType();
            entity.Property(item => item.ChangedFieldsJson).HasJsonColumnType();
        });

        modelBuilder.Entity<ContractTimelineItemEntity>(entity =>
        {
            entity.ToTable("contract_timeline_items");
            entity.HasKey(item => item.EventId);
            entity.HasIndex(item => new
            {
                item.OrganizationId,
                item.ContractId,
                item.SourceSequence
            });
            entity.Property(item => item.ChangeKind).HasChangeKindMaxLength();
            entity.Property(item => item.EntityKind).HasEntityKindMaxLength();
            entity.Property(item => item.Actor).HasActorEmailMaxLength();
            entity.Property(item => item.ChangesJson).HasJsonColumnType();
            entity.Property(item => item.DataQualityIssuesJson).HasJsonColumnType();
            entity.Property(item => item.ContractIdResolved).HasColumnName("RelationshipResolved");
        });

        modelBuilder.Entity<ContractSearchEntity>(entity =>
        {
            entity.ToTable("contract_search");
            entity.HasKey(item => new { item.OrganizationId, item.ContractId });
            entity.HasIndex(item => item.Number);
            entity.HasIndex(item => item.InternalNumber);
            entity.HasIndex(item => new { item.OrganizationId, item.LastActivityAt });
            entity.Property(item => item.Number).HasContractNumberMaxLength();
            entity.Property(item => item.InternalNumber).HasContractNumberMaxLength();
            entity.Property(item => item.Subject).HasContractSubjectMaxLength();
            entity.Property(item => item.ContractorName).HasContractorNameMaxLength();
        });

        modelBuilder.Entity<ContractSearchAliasEntity>(entity =>
        {
            entity.ToTable("contract_search_aliases");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new
            {
                item.OrganizationId,
                item.ContractId,
                item.Field,
                item.Value
            }).IsUnique();
            entity.HasIndex(item => new { item.OrganizationId, item.Value });
            entity.Property(item => item.Field).HasAliasFieldMaxLength();
            entity.Property(item => item.Value).HasAliasValueMaxLength();
        });

        modelBuilder.Entity<ImportCheckpointEntity>(entity =>
        {
            entity.ToTable("import_checkpoints");
            entity.HasKey(item => item.Source);
            entity.Property(item => item.Source).HasSourceMaxLength();
        });

        modelBuilder.Entity<LegacySynchronizationRequestEntity>(entity =>
        {
            entity.ToTable("legacy_synchronization_requests");
            entity.HasKey(item => item.Source);
            entity.HasIndex(item => item.CorrelationId).IsUnique();
            entity.Property(item => item.Source).HasMaxLength(100);
        });

        modelBuilder.Entity<ContractArchiveTransferEntity>(entity =>
        {
            entity.ToTable("contract_archive_transfers");
            entity.HasKey(item => new { item.OrganizationId, item.ContractId });
            entity.HasIndex(item => new
            {
                item.State,
                item.UpdatedAt
            });
            entity.Property(item => item.State).HasMaxLength(40);
            entity.Property(item => item.ErrorCode).HasMaxLength(200);
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
