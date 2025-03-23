using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Outbox;

namespace TechShelf.IntegrationTests.Infrastructure.Data.Outbox;

public class OutboxMessageProcessorIntegrationTests : PostgresContainerTestBase
{
    private readonly FakeLogger<OutboxMessageProcessor> _logger;
    private readonly TestEventHandler _testEventHandler;
    private ApplicationDbContext _dbContext;
    private IServiceProvider _serviceProvider;
    private OutboxMessageProcessor _processor;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public OutboxMessageProcessorIntegrationTests()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _testEventHandler = new TestEventHandler();
        _logger = new FakeLogger<OutboxMessageProcessor>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // Setup database context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        // Setup service provider
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseNpgsql(ConnectionString));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TestDomainEvent).Assembly));
        services.AddSingleton<INotificationHandler<TestDomainEvent>>(_testEventHandler);

        _serviceProvider = services.BuildServiceProvider();

        // Initialize the processor after service provider is built
        _processor = new OutboxMessageProcessor(_serviceProvider, _logger);
    }

    public override async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesSuccessfully_WithValidMessage()
    {
        // Arrange
        var testEvent = new TestDomainEvent { Id = Guid.NewGuid(), Data = "Test Data" };
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).FullName!,
            Content = JsonSerializer.Serialize(testEvent, testEvent.GetType()),
            ProcessedOn = null,
            RetryCount = 0
        };

        await _dbContext.OutboxMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        await _processor.ExecuteAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var processedMessage = await _dbContext.OutboxMessages.FindAsync(message.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOn.Should().NotBeNull();
        processedMessage.RetryCount.Should().Be(0);
        processedMessage.Error.Should().BeNull();

        _logger.Collector.GetSnapshot().Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains($"Successfully processed outbox message {message.Id}"));

        // Verify the event was actually handled
        _testEventHandler.HandledEvents.Should().Contain(e =>
            e.Id == testEvent.Id &&
            e.Data == testEvent.Data);
    }

    [Fact]
    public async Task ExecuteAsync_IncreasesRetryCount_WithInvalidType()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "InvalidType1234567890",
            Content = "{}",
            ProcessedOn = null,
            RetryCount = 0
        };

        await _dbContext.OutboxMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        await _processor.ExecuteAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var processedMessage = await _dbContext.OutboxMessages.FindAsync(message.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOn.Should().BeNull();
        processedMessage.RetryCount.Should().Be(1);
        processedMessage.Error.Should().Contain("Could not load event type");

        _logger.Collector.GetSnapshot().Should().Contain(e =>
            e.Level == LogLevel.Error &&
            e.Message.Contains($"Error processing outbox message {message.Id}"));
    }

    [Fact]
    public async Task ExecuteAsync_SkipsMessage_WithMaxRetryCount()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).ToString(),
            Content = "{}",
            ProcessedOn = null,
            RetryCount = 5
        };

        await _dbContext.OutboxMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // Act
        await _processor.ExecuteAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var processedMessage = await _dbContext.OutboxMessages.FindAsync(message.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOn.Should().BeNull();
        processedMessage.RetryCount.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesAllSuccessfully_WithMultipleMessages()
    {
        // Arrange
        var messages = Enumerable.Range(0, 3).Select(_ => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).FullName!,
            Content = JsonSerializer.Serialize(new TestDomainEvent
            {
                Id = Guid.NewGuid(),
                Data = "Test Data"
            }),
            ProcessedOn = null,
            RetryCount = 0
        }).ToList();

        await _dbContext.OutboxMessages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        // Act
        await _processor.ExecuteAsync();
        _dbContext.ChangeTracker.Clear();

        // Assert
        var processedMessages = await _dbContext.OutboxMessages.ToListAsync();
        processedMessages.Should().HaveCount(3);
        processedMessages.Should().AllSatisfy(m =>
        {
            m.ProcessedOn.Should().NotBeNull();
            m.RetryCount.Should().Be(0);
            m.Error.Should().BeNull();
        });

        _testEventHandler.HandledEvents.Should().HaveCount(3);
    }
}

// Test classes
public class TestDomainEvent : INotification
{
    public Guid Id { get; set; }
    public string Data { get; set; } = string.Empty;
}

public class TestEventHandler : INotificationHandler<TestDomainEvent>
{
    private readonly List<TestDomainEvent> _handledEvents = new();
    public IReadOnlyList<TestDomainEvent> HandledEvents => _handledEvents;

    public Task Handle(TestDomainEvent notification, CancellationToken cancellationToken)
    {
        _handledEvents.Add(notification);
        return Task.CompletedTask;
    }
}
