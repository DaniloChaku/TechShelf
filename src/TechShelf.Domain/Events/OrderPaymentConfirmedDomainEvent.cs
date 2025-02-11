using TechShelf.Domain.Common;

namespace TechShelf.Domain.Events;

public record OrderPaymentConfirmedDomainEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;
