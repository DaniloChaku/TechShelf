﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TechShelf.Infrastructure.Data.Outbox;

public class OutboxMessageProcessor : IOutboxMessageProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessor> _logger;
    private const int MaxRetryCount = 5;

    public OutboxMessageProcessor(IServiceProvider serviceProvider, ILogger<OutboxMessageProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var messages = await dbContext.OutboxMessages
            .Where(m => !m.ProcessedOn.HasValue && m.RetryCount < MaxRetryCount)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    throw new InvalidOperationException($"Could not load event type: {message.Type}");
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as INotification;
                if (domainEvent == null)
                {
                    throw new InvalidOperationException($"Unable to deserialize domain event for message {message.Id}");
                }

                await mediator.Publish(domainEvent, cancellationToken);

                message.ProcessedOn = DateTime.UtcNow;
                _logger.LogInformation("Successfully processed outbox message {MessageId}.", message.Id);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                _logger.LogError(
                    ex,
                    "Error processing outbox message {MessageId}. Retry count is now {RetryCount}.",
                    message.Id,
                    message.RetryCount);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
