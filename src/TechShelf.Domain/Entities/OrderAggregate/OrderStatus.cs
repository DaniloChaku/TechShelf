namespace TechShelf.Domain.Entities.OrderAggregate;

public enum OrderStatus
{
    PaymentPending,
    PaymentSucceeded,
    PaymentFailed,
    Processing,
    Shipping,
    Shipped,
    ReceivedByCustomer,
    RefundRequested,
    Refunded,
}
