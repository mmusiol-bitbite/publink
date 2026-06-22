namespace Audit.Application.Archiving;

public sealed class ContractArchivalEligibilityPolicy
{
    public ContractArchivalEligibilityPolicy(TimeProvider timeProvider, int inactivityMonths)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        if (inactivityMonths <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inactivityMonths),
                inactivityMonths,
                "Inactivity months must be positive.");
        }

        Cutoff = timeProvider.GetUtcNow().AddMonths(-inactivityMonths);
    }

    public DateTimeOffset Cutoff { get; }

    public bool IsSatisfiedBy(DateTimeOffset lastActivityAt) => lastActivityAt < Cutoff;
}
