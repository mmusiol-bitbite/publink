using Audit.Domain;

namespace Audit.Domain.Tests;

public sealed class TolerantCodeTests
{
    private readonly Fixture fixture = new();

    [Theory]
    [InlineData(1, "added")]
    [InlineData(2, "deleted")]
    [InlineData(3, "modified")]
    [InlineData(0, "unknown")]
    [InlineData(99, "unknown")]
    public void WhenResolvingChangeKindGivenKnownOrUnknownCodeThenPreservesCodeAndReturnsExpectedKey(int code, string expectedKey)
    {
        var kind = ChangeKind.FromCode(code);

        kind.Code.Should().Be(code);
        kind.Key.Should().Be(expectedKey);
    }

    [Fact]
    public void WhenResolvingEntityKindGivenSchemaDriftCodeThenPreservesUnknownCode()
    {
        var schemaDriftCode = fixture.Create<int>() + 100;

        var kind = AuditedEntityKind.FromCode(schemaDriftCode);

        kind.Code.Should().Be(schemaDriftCode);
        kind.Key.Should().Be("unknown");
    }
}
