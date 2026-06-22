namespace Audit.Application.Archiving;

public sealed class ContractArchivalOptions
{
    public const string SectionName = "Archival";

    public int InactivityMonths { get; init; } = 18;

    public TimeSpan RunInterval { get; init; } = TimeSpan.FromHours(24);

    public int MaximumContractsPerRun { get; init; } = 100;
}
