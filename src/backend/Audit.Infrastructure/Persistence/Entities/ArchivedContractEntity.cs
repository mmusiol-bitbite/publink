namespace Audit.Infrastructure.Persistence.Entities;

public sealed class ArchivedContractEntity
{
    public Guid OrganizationId { get; set; }

    public Guid ContractId { get; set; }

    public DateTimeOffset ArchivedAt { get; set; }

    public DateTimeOffset LastActivityAt { get; set; }

    public long LastSourceSequence { get; set; }

    public string? Number { get; set; }

    public string? InternalNumber { get; set; }

    public string? Subject { get; set; }

    public string? ContractorName { get; set; }
}
