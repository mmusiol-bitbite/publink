using System.Text.Json;
using Audit.Application.Exports;
using Audit.Application.Legacy;
using Audit.Application.Queries;
using Audit.Query.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Query.Api.Tests;

public sealed class ApiExceptionHandlerTests
{
    private readonly Fixture fixture = new();

    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public async Task WhenHandlingExceptionGivenKnownOperationalExceptionThenWritesProblemDetails(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle)
    {
        var handler = new ApiExceptionHandler();
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        var traceId = fixture.Create<string>();
        var httpContext = new DefaultHttpContext { TraceIdentifier = traceId };
        httpContext.RequestServices = serviceProvider;
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
        responseBody.Position = 0;
        using var problem = await JsonDocument.ParseAsync(responseBody);
        problem.RootElement.GetProperty("title").GetString().Should().Be(expectedTitle);
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(expectedStatusCode);
        problem.RootElement.GetProperty("traceId").GetString().Should().Be(traceId);
    }

    public static TheoryData<Exception, int, string> ExceptionMappings() =>
        new()
        {
            { new ArgumentException("Bad query."), StatusCodes.Status400BadRequest, "invalidRequest" },
            { new ExportTooLargeException(10), StatusCodes.Status413PayloadTooLarge, "exportTooLarge" },
            { new LegacySynchronizationUnavailableException("Unavailable."), StatusCodes.Status503ServiceUnavailable, "synchronizationUnavailable" },
            { new ContractStoreUnavailableException("Store unavailable."), StatusCodes.Status503ServiceUnavailable, "contractStoreUnavailable" },
            { new InvalidOperationException("Unexpected."), StatusCodes.Status500InternalServerError, "unexpectedError" }
        };
}
