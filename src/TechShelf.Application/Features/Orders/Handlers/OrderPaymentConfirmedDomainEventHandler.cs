using MediatR;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Domain.Orders.Events;

namespace TechShelf.Application.Features.Orders.Handlers;

public class OrderPaymentConfirmedDomainEventHandler
    : INotificationHandler<OrderPaymentConfirmedDomainEvent>
{
    private readonly IEmailService _emailService;

    public OrderPaymentConfirmedDomainEventHandler(
        IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        OrderPaymentConfirmedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        string subject = "Order Payment Confirmation";
        string message =
            $"Thank you for your payment! Your order #{notification.OrderId} has been confirmed and is being processed.";

        await _emailService.SendPlainTextEmailAsync(
            notification.CustomerEmail,
            subject,
            message,
            cancellationToken);
    }
}
