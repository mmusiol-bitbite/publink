using Audit.Application.Archiving;

namespace Audit.Application.Tests;

public sealed class ContractArchivalEligibilityPolicyTests
{
    private readonly Fixture fixture;
    private readonly TimeProvider timeProvider;

    public ContractArchivalEligibilityPolicyTests()
    {
        fixture = new Fixture();
        timeProvider = new FrozenTimeProvider(fixture.Create<DateTimeOffset>());
    }

    [Fact]
    public void WhenCheckingEligibilityGivenActivityAtCutoffThenRequiresActivityOlderThanCutoff()
    {
        var policy = new ContractArchivalEligibilityPolicy(timeProvider, 18);

        var now = timeProvider.GetUtcNow();
        policy.IsSatisfiedBy(now.AddMonths(-18).AddTicks(-1)).Should().BeTrue();
        policy.IsSatisfiedBy(now.AddMonths(-18)).Should().BeFalse();
        policy.IsSatisfiedBy(now.AddMonths(-17)).Should().BeFalse();
    }

    [Fact]
    public void WhenCreatingPolicyGivenNonPositiveRetentionThenRejectsConfiguration()
    {
        var act = () => new ContractArchivalEligibilityPolicy(TimeProvider.System, -Math.Abs(fixture.Create<int>()));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private sealed class FrozenTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

