using Audit.Infrastructure.Persistence.Entities;

namespace Audit.Infrastructure.Archiving.Queries;

internal static class ContractArchiveQueryExtensions
{
    public static IQueryable<ContractSearchEntity> WhereContract(
        this IQueryable<ContractSearchEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ContractSearchAliasEntity> WhereContract(
        this IQueryable<ContractSearchAliasEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ContractTimelineItemEntity> WhereContract(
        this IQueryable<ContractTimelineItemEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ContractArchiveTransferEntity> WhereTransfer(
        this IQueryable<ContractArchiveTransferEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ArchivedContractEntity> WhereContract(
        this IQueryable<ArchivedContractEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ArchivedContractAliasEntity> WhereContract(
        this IQueryable<ArchivedContractAliasEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ArchivedAuditEventEntity> WhereContract(
        this IQueryable<ArchivedAuditEventEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);

    public static IQueryable<ArchivedTimelineItemEntity> WhereContract(
        this IQueryable<ArchivedTimelineItemEntity> query,
        Guid organizationId,
        Guid contractId) =>
        query.Where(item => item.OrganizationId == organizationId && item.ContractId == contractId);
}
