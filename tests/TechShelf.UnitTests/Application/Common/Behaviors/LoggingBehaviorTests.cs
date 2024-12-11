using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using TechShelf.Application.Common.Behaviors;

namespace TechShelf.UnitTests.Application.Common.Behaviors;


public class LoggingBehaviorTests
{
    private readonly FakeLogger<LoggingBehavior<SampleRequest, SampleResponse>> _fakeLogger;
    private readonly LoggingBehavior<SampleRequest, SampleResponse> _behavior;
    private readonly Fixture _fixture;

    public LoggingBehaviorTests()
    {
        _fakeLogger = new FakeLogger<LoggingBehavior<SampleRequest, SampleResponse>>();
        _behavior = new LoggingBehavior<SampleRequest, SampleResponse>(_fakeLogger);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_LogsRequestAndResponseCorrectly()
    {
        // Arrange
        var request = _fixture.Create<SampleRequest>();
        var response = _fixture.Create<SampleResponse>();
        var expectedRequestTypeName = typeof(SampleRequest).Name;
        var nextDelegate = new RequestHandlerDelegate<SampleResponse>(() => Task.FromResult(response));

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _behavior.Handle(request, nextDelegate, cancellationToken);

        // Assert
        result.Should().Be(response);

        _fakeLogger.Collector.Count.Should().Be(2);
        _fakeLogger.Collector.GetSnapshot()
            .Should()
            .ContainSingle(log =>
            log.Level == LogLevel.Information &&
            log.Message == $"Handling {expectedRequestTypeName}: {request}");
        _fakeLogger.Collector.GetSnapshot()
            .Should()
            .ContainSingle(log =>
            log.Level == LogLevel.Information &&
            log.Message == $"Handled {expectedRequestTypeName} in {It.IsAny<int>()}ms. Response: {response}");
    }

    private class SampleRequest : IRequest<SampleResponse>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
