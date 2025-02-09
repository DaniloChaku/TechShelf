using TechShelf.Domain.Common;

namespace TechShelf.Domain.Entities.OrderAggregate;

public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _orderItems;
    private readonly List<OrderHistoryEntry> _history;

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
    public IReadOnlyList<OrderHistoryEntry> History => _history.AsReadOnly();
    public Address Address { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string FullName { get; private set; }
    public string? CustomerId { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public decimal Total { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Order() { } // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Order(string email, string phoneNumber, string fullName, Address address, IEnumerable<OrderItem> orderItems, string customerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(orderItems);
        ArgumentNullException.ThrowIfNull(customerId);
        if (!orderItems.Any())
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        Id = Guid.NewGuid();
        Email = email;
        PhoneNumber = phoneNumber;
        FullName = fullName;
        Address = address;
        _orderItems = orderItems.ToList();
        CustomerId = customerId;

        var initialHistoryEntry = new OrderHistoryEntry(Id, OrderStatus.PaymentPending);
        _history = [initialHistoryEntry];

        Total = _orderItems.Sum(x => x.GetTotal());
    }

    public void SetPaymentStatus(bool isPaymentSuccessful, string paymentIntentId)
    {
        var currentStatus = _history[^1].Status;
        if (currentStatus is not (OrderStatus.PaymentPending or OrderStatus.PaymentFailed))
        {
            throw new InvalidOperationException(
                $"Cannot process payment for order {Id} - payment already processed (current status: {_history[^1].Status}).");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(paymentIntentId);
        PaymentIntentId = paymentIntentId;

        _history.Add(new OrderHistoryEntry(
            Id,
            isPaymentSuccessful ? OrderStatus.PaymentSucceeded : OrderStatus.PaymentFailed
        ));
    }
}
