using TechShelf.Domain.Common;

namespace TechShelf.Domain.Entities.OrderAggregate;

public class Order : Entity<Guid>
{
    private readonly List<OrderItem> _orderItems;
    private readonly List<OrderHistoryEntry> _history;

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
    public IReadOnlyList<OrderHistoryEntry> History => _history.AsReadOnly();
    public Address Address { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public decimal Total { get; private set; }
    public string? PaymentIntentId { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Order() { } // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Order(string email, string phoneNumber, Address address, IEnumerable<OrderItem> orderItems)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(orderItems);
        if (!orderItems.Any())
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        Id = Guid.NewGuid();
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        _orderItems = orderItems.ToList();

        var initialHistoryEntry = new OrderHistoryEntry(Id, OrderStatus.PaymentPending);
        _history = [initialHistoryEntry];

        Total = _orderItems.Sum(x => x.GetTotal());
    }

    public void SetStripePaymentIntentId(string stripePaymentIntentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripePaymentIntentId);
        PaymentIntentId = stripePaymentIntentId;
    }

    public void AddHistoryEntry(OrderHistoryEntry historyEntry)
    {
        ArgumentNullException.ThrowIfNull(historyEntry);
        _history.Add(historyEntry);
    }
}
