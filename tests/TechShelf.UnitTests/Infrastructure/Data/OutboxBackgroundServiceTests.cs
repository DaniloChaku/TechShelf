using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using TechShelf.Infrastructure.Data.Outbox;

namespace TechShelf.UnitTests.Infrastructure.Data;

public class OutboxBackgroundServiceTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IOutboxMessageProcessor> _outboxProcessorMock;
    private readonly FakeLogger<OutboxBackgroundService> _logger;
    private readonly OutboxOptions _outboxOptions;

    public OutboxBackgroundServiceTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _outboxProcessorMock = new Mock<IOutboxMessageProcessor>();
        _logger = new FakeLogger<OutboxBackgroundService>();

        _outboxOptions = new OutboxOptions { OutboxProcessorFrequencyMilliseconds = 200 };

        _serviceScopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _serviceProviderMock
            .Setup(p => p.GetService(typeof(IOutboxMessageProcessor)))
            .Returns(_outboxProcessorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesOutboxMessages_UntilCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var service = new OutboxBackgroundService(_serviceScopeFactoryMock.Object, Options.Create(_outboxOptions), _logger);

        _outboxProcessorMock
            .Setup(p => p.ExecuteAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(500);
        await cts.CancelAsync();
        await executeTask; 

        // Assert
        _outboxProcessorMock.Verify(p => p.ExecuteAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteAsync_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var service = new OutboxBackgroundService(_serviceScopeFactoryMock.Object, Options.Create(_outboxOptions), _logger);

        _outboxProcessorMock
            .Setup(p => p.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(5); // Allow execution for a short time
        await cts.CancelAsync();

        // Assert
        _logger.Collector.GetSnapshot().Should().Contain(e => e.Message.Contains("An error occurred in the Outbox background service"));
    }

    [Fact]
    public async Task ExecuteAsync_LogsCancellation_WhenStopped()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var service = new OutboxBackgroundService(_serviceScopeFactoryMock.Object, Options.Create(_outboxOptions), _logger);

        // Act
        await service.StartAsync(cts.Token);
        await cts.CancelAsync();
        await Task.Delay(10);

        // Assert
        _logger.Collector.GetSnapshot().Should().Contain(e => e.Message.Contains("Outbox background service cancelled."));
    }
}
