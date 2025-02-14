using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechShelf.Infrastructure.Data.Outbox;
using TechShelf.Infrastructure.Data;
using Microsoft.Extensions.Logging.Testing;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace TechShelf.IntegrationTests.Infrastructure.Data.Outbox;

public class OutboxMessageProcessorIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly FakeLogger<OutboxMessageProcessor> _logger;
    private readonly OutboxMessageProcessor _processor;
    private readonly TestEventHandler _testEventHandler;

    public OutboxMessageProcessorIntegrationTests()
    {
        // Database setup
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<OutboxMessageProcessorIntegrationTests>()
            .Build();

        var connectionString = configuration["ConnectionStrings:TestDatabase"];
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, o => o.SetPostgresVersion(12, 0))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _testEventHandler = new TestEventHandler();

        // Service provider setup
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseNpgsql(connectionString, o => o.SetPostgresVersion(12, 0)));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TestDomainEvent).Assembly));
        services.AddSingleton<INotificationHandler<TestDomainEvent>>(_testEventHandler);

        _serviceProvider = services.BuildServiceProvider();
        _logger = new FakeLogger<OutboxMessageProcessor>();
        _processor = new OutboxMessageProcessor(_serviceProvider, _logger);
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

    private bool _disposed;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Database.EnsureDeleted();
                _dbContext.Dispose();
            }

            _disposed = true;
        }
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
