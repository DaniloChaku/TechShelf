using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Data.Interceptors;
using TechShelf.Infrastructure.Data.Outbox;

namespace TechShelf.IntegrationTests.Infrastructure.Data.Interceptors;

public class DomainEventsToOutboxInterceptorTests : PostgresContainerTestBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TestDbContext _dbContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private readonly Fixture _fixture = new();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(new DomainEventsToOutboxInterceptor())
            .Options;
        _dbContext = new TestDbContext(options);

        await _dbContext.Database.EnsureCreatedAsync();
    }

    public override async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task SavingChangesAsync_AddsOutboxMessages_WhenAggregateHasDomainEvents()
    {
        // Arrange
        var fakeAggregate = new FakeAggregate();
        var message = _fixture.Create<string>();
        var dummyEvent = new DummyDomainEvent(message);
        fakeAggregate.Raise(dummyEvent);

        await _dbContext.Set<FakeAggregate>().AddAsync(fakeAggregate);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var outboxMessages = await _dbContext.Set<OutboxMessage>().ToListAsync();
        outboxMessages.Should().HaveCount(1);

        var outboxMessage = outboxMessages[0];
        outboxMessage.Type.Should().Be(typeof(DummyDomainEvent).FullName);
        var deserializedEvent = JsonSerializer.Deserialize<DummyDomainEvent>(outboxMessage.Content);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Message.Should().Be(message);

        fakeAggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotAddOutboxMessages_WhenAggregateHasNoDomainEvents()
    {
        // Arrange
        var fakeAggregate = new FakeAggregate();

        await _dbContext.Set<FakeAggregate>().AddAsync(fakeAggregate);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var outboxMessages = await _dbContext.Set<OutboxMessage>().ToListAsync();
        outboxMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChangesAsync_AddsOutboxMessagesForThoseWithEvents_WhenMultipleAggregatesWithAndWithoutDomainEvents()
    {
        // Arrange

        // Aggregate without any domain events.
        var aggregateWithoutEvents = new FakeAggregate();

        // Aggregate with a single domain event.
        var aggregateWithSingleEvent = new FakeAggregate();
        var message1 = _fixture.Create<string>();
        aggregateWithSingleEvent.Raise(new DummyDomainEvent(message1));

        // Aggregate with multiple domain events.
        var aggregateWithMultipleEvents = new FakeAggregate();
        var message2 = _fixture.Create<string>();
        var message3 = _fixture.Create<string>();
        aggregateWithMultipleEvents.Raise(new DummyDomainEvent(message2));
        aggregateWithMultipleEvents.Raise(new DummyDomainEvent(message3));

        await _dbContext.Set<FakeAggregate>().AddRangeAsync(
            aggregateWithoutEvents,
            aggregateWithSingleEvent,
            aggregateWithMultipleEvents
        );

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var outboxMessages = await _dbContext.Set<OutboxMessage>().ToListAsync();
        outboxMessages.Should().HaveCount(3);

        outboxMessages.ForEach(msg => msg.Type.Should().Be(typeof(DummyDomainEvent).FullName));

        var deserializedEvents = outboxMessages
            .Select(msg => JsonSerializer.Deserialize<DummyDomainEvent>(msg.Content))
            .ToList();

        deserializedEvents.Should().Contain(e => e!.Message == message1);
        deserializedEvents.Should().Contain(e => e!.Message == message2);
        deserializedEvents.Should().Contain(e => e!.Message == message3);

        aggregateWithSingleEvent.DomainEvents.Should().BeEmpty();
        aggregateWithMultipleEvents.DomainEvents.Should().BeEmpty();
    }
}

public class FakeAggregate : AggregateRoot<int>
{
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    { }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<FakeAggregate> FakeAggregates { get; set; }
}

public record DummyDomainEvent(string Message) : IDomainEvent;
