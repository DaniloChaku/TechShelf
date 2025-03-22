using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using TechShelf.API.Common;

namespace TechShelf.UnitTests.Api.Common;

public class GlobalExceptionHandlerTests
{
    private readonly FakeLogger<GlobalExceptionHandler> _fakeLogger;
    private readonly Mock<ProblemDetailsFactory> _mockProblemDetailsFactory;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _fakeLogger = new FakeLogger<GlobalExceptionHandler>();
        _mockProblemDetailsFactory = new Mock<ProblemDetailsFactory>();
        _handler = new GlobalExceptionHandler(_fakeLogger, _mockProblemDetailsFactory.Object);
    }

    [Fact]
    public async Task TryHandleAsync_LogsTheExceptionAndReturnsProblemDetails()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        var exception = new InvalidOperationException("Test exception");
        var cancellationToken = CancellationToken.None;

        var expectedProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error",
            Detail = "An error occurred while processing the request"
        };

        _mockProblemDetailsFactory
            .Setup(f => f.CreateProblemDetails(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "Server error",
                null,
                "An error occurred while processing the request",
                It.IsAny<string>()))
            .Returns(expectedProblemDetails);

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, cancellationToken);

        // Assert
        result.Should().BeTrue();

        // Validate logs
        _fakeLogger.Collector.Count.Should().Be(1);
        _fakeLogger.Collector.GetSnapshot()
            .Should()
            .ContainSingle(log =>
                log.Level == LogLevel.Error &&
                log.Message.Contains(exception.Message, StringComparison.OrdinalIgnoreCase));

        // Validate response
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        responseStream.Seek(0, SeekOrigin.Begin);
        var actualProblemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            responseStream);

        actualProblemDetails.Should().BeEquivalentTo(expectedProblemDetails);
    }
}
