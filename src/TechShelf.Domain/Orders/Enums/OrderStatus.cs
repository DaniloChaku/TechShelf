namespace TechShelf.Domain.Orders.Enums;

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
