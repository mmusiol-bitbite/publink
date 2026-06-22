using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Snapshots;

public sealed record ArchivedContractSnapshot(
    ArchivedContractEntity Contract,
    IReadOnlyCollection<ArchivedContractAliasEntity> Aliases,
    IReadOnlyCollection<ArchivedAuditEventEntity> Events,
    IReadOnlyCollection<ArchivedTimelineItemEntity> Timeline);
