using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;

public record SetPaymentStatusCommand(
    Guid OrderId,
    bool IsPaymentSuccessful,
    string PaymentIntentId)
    : IRequest<ErrorOr<Unit>>;
