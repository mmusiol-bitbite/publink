namespace Audit.Infrastructure.Persistence.Entities;

public sealed class ContractSearchAliasEntity
{
    public long Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ContractId { get; set; }

    public required string Field { get; set; }

    public required string Value { get; set; }

    public bool IsCurrent { get; set; }
}
