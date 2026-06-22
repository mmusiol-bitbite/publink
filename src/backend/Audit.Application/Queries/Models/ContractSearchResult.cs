namespace Audit.Application.Queries;

public sealed record ContractSearchResult(
    Guid ContractId,
    string? Number,
    string? InternalNumber,
    string? Subject,
    string? ContractorName,
    DateTimeOffset LastActivityAt,
    bool MatchedHistoricalValue);
