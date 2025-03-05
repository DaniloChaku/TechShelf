using TechShelf.Domain.Common;

namespace TechShelf.Domain.Orders.Events;

public record OrderPaymentConfirmedDomainEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;
