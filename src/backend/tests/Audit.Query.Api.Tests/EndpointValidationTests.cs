using Audit.Query.Api.Endpoints.Requests;
using Audit.Query.Api.Endpoints.Validation;

namespace Audit.Query.Api.Tests;

public sealed class EndpointValidationTests
{
    private readonly Fixture fixture = new();

    [Fact]
    public void WhenValidatingSearchGivenPhraseIsMissingOrTooShortThenReturnsValidationError()
    {
        var validator = new SearchQueryValidator();
        var query = fixture.Build<SearchQuery>()
            .With(request => request.SearchPhrase, "  a  ")
            .Create();

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.ToProblemDetailsErrors().Should().ContainKey("searchPhrase")
            .WhoseValue.Should().BeEquivalentTo(["searchQueryTooShort"]);
    }

    [Fact]
    public void WhenValidatingSearchGivenTrimmedPhraseHasMinimumLengthThenAcceptsRequest()
    {
        var validator = new SearchQueryValidator();
        var query = fixture.Build<SearchQuery>()
            .With(request => request.SearchPhrase, "  AB  ")
            .Create();

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.ToProblemDetailsErrors().Should().BeEmpty();
    }

    [Fact]
    public void WhenValidatingTimelineGivenInvalidRangeAndCodesThenReturnsAllFilterErrors()
    {
        var validator = new TimelineQueryValidator();
        var query = fixture.Build<TimelineQuery>()
            .With(request => request.From, new DateTimeOffset(2026, 6, 22, 12, 0, 0, TimeSpan.Zero))
            .With(request => request.To, new DateTimeOffset(2026, 6, 21, 12, 0, 0, TimeSpan.Zero))
            .With(request => request.Actor, new string('x', 201))
            .With(request => request.ChangeType, 999)
            .With(request => request.EntityType, 999)
            .Create();

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.ToProblemDetailsErrors().Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["dateRange"] = ["timelineDateRangeInvalid"],
            ["actor"] = ["timelineActorTooLong"],
            ["changeType"] = ["timelineChangeTypeInvalid"],
            ["entityType"] = ["timelineEntityTypeInvalid"]
        });
    }

    [Fact]
    public void WhenValidatingExportGivenTimelineErrorsAndUnsupportedLocaleThenReturnsCombinedErrors()
    {
        var validator = new ExportQueryValidator(new TimelineQueryValidator());
        var query = fixture.Build<ExportQuery>()
            .With(request => request.From, new DateTimeOffset(2026, 6, 22, 12, 0, 0, TimeSpan.Zero))
            .With(request => request.To, new DateTimeOffset(2026, 6, 21, 12, 0, 0, TimeSpan.Zero))
            .With(request => request.Locale, "de")
            .Create();

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.ToProblemDetailsErrors().Should().ContainKey("dateRange")
            .WhoseValue.Should().BeEquivalentTo(["timelineDateRangeInvalid"]);
        result.ToProblemDetailsErrors().Should().ContainKey("locale")
            .WhoseValue.Should().BeEquivalentTo(["exportLocaleInvalid"]);
    }

    [Theory]
    [InlineData("pl")]
    [InlineData("PL")]
    [InlineData("en")]
    [InlineData("EN")]
    public void WhenValidatingExportGivenSupportedLocaleWithAnyCaseThenAcceptsRequest(string locale)
    {
        var validator = new ExportQueryValidator(new TimelineQueryValidator());
        var query = fixture.Build<ExportQuery>()
            .With(request => request.Locale, locale)
            .With(request => request.From, (DateTimeOffset?)null)
            .With(request => request.To, (DateTimeOffset?)null)
            .With(request => request.Actor, (string?)null)
            .With(request => request.ChangeType, (int?)null)
            .With(request => request.EntityType, (int?)null)
            .Create();

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
