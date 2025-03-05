using TechShelf.Domain.Common;
using TechShelf.Domain.Orders.Enums;

namespace TechShelf.Domain.Orders;

public class OrderHistoryEntry : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime Date { get; private set; }
    public string? Notes { get; private set; }

    private OrderHistoryEntry() { } // For EF Core

    public OrderHistoryEntry(Guid orderId, OrderStatus status, string? notes = null)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("OrderId must not be empty.");
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        Status = status;
        Date = DateTime.UtcNow;
        Notes = notes;
    }
}
