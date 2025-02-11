using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using TechShelf.Domain.Common;

namespace TechShelf.Infrastructure.Data.Interceptors;

public class DomainEventsToOutboxInterceptor
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        SaveDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await SaveDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task SaveDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var aggregates = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(a => a.Entity.DomainEvents.Count != 0)
            .Select(a => a.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => {
                List<IDomainEvent> domainEvents = [..a.DomainEvents];
                a.ClearDomainEvents();

                return domainEvents;
            })
            .Select(d => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = d.GetType().Name,
                Content = JsonSerializer.Serialize(d, d.GetType()),
                OccurredOn = DateTime.UtcNow,

            })
            .ToList();

        await context.Set<OutboxMessage>().AddRangeAsync(domainEvents);
    }
}
