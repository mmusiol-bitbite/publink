using Audit.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence.Contexts;

public sealed class ArchiveDbContext(DbContextOptions<ArchiveDbContext> options)
    : DbContext(options)
{
    public DbSet<ArchivedContractEntity> Contracts => Set<ArchivedContractEntity>();

    public DbSet<ArchivedContractAliasEntity> ContractAliases => Set<ArchivedContractAliasEntity>();

    public DbSet<ArchivedAuditEventEntity> AuditEvents => Set<ArchivedAuditEventEntity>();

    public DbSet<ArchivedTimelineItemEntity> TimelineItems => Set<ArchivedTimelineItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<ArchivedContractEntity>(entity =>
        {
            entity.ToTable("archived_contracts");
            entity.HasKey(item => new { item.OrganizationId, item.ContractId });
            entity.HasIndex(item => new { item.OrganizationId, item.LastActivityAt });
            entity.HasIndex(item => item.Number);
            entity.HasIndex(item => item.InternalNumber);
            entity.Property(item => item.Number).HasContractNumberMaxLength();
            entity.Property(item => item.InternalNumber).HasContractNumberMaxLength();
            entity.Property(item => item.Subject).HasContractSubjectMaxLength();
            entity.Property(item => item.ContractorName).HasContractorNameMaxLength();
        });

        modelBuilder.Entity<ArchivedContractAliasEntity>(entity =>
        {
            entity.ToTable("archived_contract_aliases");
            entity.HasKey(item => new
            {
                item.OrganizationId,
                item.ContractId,
                item.Field,
                item.Value
            });
            entity.HasIndex(item => new { item.OrganizationId, item.Value });
            entity.Property(item => item.Field).HasAliasFieldMaxLength();
            entity.Property(item => item.Value).HasAliasValueMaxLength();
        });

        modelBuilder.Entity<ArchivedAuditEventEntity>(entity =>
        {
            entity.ToTable("archived_audit_events");
            entity.HasKey(item => item.EventId);
            entity.HasIndex(item => new { item.OrganizationId, item.Source, item.SourceEventId }).IsUnique();
            entity.HasIndex(item => new { item.OrganizationId, item.SourceSequence });
            entity.Property(item => item.Source).HasSourceMaxLength();
            entity.Property(item => item.ActorEmail).HasActorEmailMaxLength();
            entity.Property(item => item.BeforeJson).HasJsonColumnType();
            entity.Property(item => item.AfterJson).HasJsonColumnType();
            entity.Property(item => item.ChangedFieldsJson).HasJsonColumnType();
        });

        modelBuilder.Entity<ArchivedTimelineItemEntity>(entity =>
        {
            entity.ToTable("archived_timeline_items");
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
        });
    }
}
