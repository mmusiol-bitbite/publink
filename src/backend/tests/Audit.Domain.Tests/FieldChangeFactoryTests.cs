using Audit.Domain;

namespace Audit.Domain.Tests;

public sealed class FieldChangeFactoryTests
{
    private readonly Fixture fixture = new();

    [Fact]
    public void WhenCreatingChangesGivenAffectedFieldsThenPreservesInvariantValues()
    {
        var unchangedValue = fixture.Create<string>();

        var result = FieldChangeFactory.Create(
            $$"""{"Number":"UM/17","ContractGrossValue":120000,"Ignored":"{{unchangedValue}}"}""",
            $$"""{"Number":"UM/17A","ContractGrossValue":135000,"Ignored":"{{unchangedValue}}"}""",
            ["Number", "ContractGrossValue"]);

        result.DataQualityIssues.Should().BeEmpty();
        result.Changes.Should().BeEquivalentTo(
            [
                new FieldChange("ContractGrossValue", "120000", "135000", "number"),
                new FieldChange("Number", "UM/17", "UM/17A", "string")
            ],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void WhenCreatingChangesGivenInvalidPayloadThenReportsIssueWithoutLosingValidSide()
    {
        var observedStatus = fixture.Create<string>();

        var result = FieldChangeFactory.Create(
            "not-json",
            $$"""{"Status":"{{observedStatus}}"}"""
        );

        result.DataQualityIssues.Should().Contain("invalidBeforeJson");
        result.Changes.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new FieldChange("Status", null, observedStatus, "string"));
    }

    [Fact]
    public void WhenCreatingChangesGivenUnchangedValuesThenReturnsNoChanges()
    {
        var status = fixture.Create<string>();

        var result = FieldChangeFactory.Create(
            $$"""{"Status":"{{status}}"}""",
            $$"""{"Status":"{{status}}"}"""
        );

        result.Changes.Should().BeEmpty();
    }
}
