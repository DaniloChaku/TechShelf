namespace TechShelf.Infrastructure.Data.Outbox;

public interface IOutboxMessageProcessor
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
