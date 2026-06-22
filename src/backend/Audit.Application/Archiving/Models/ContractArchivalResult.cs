namespace Audit.Application.Archiving;

public sealed record ContractArchivalResult(
    bool Processed,
    Guid? OrganizationId,
    Guid? ContractId,
    string? Action,
    int EventCount);
