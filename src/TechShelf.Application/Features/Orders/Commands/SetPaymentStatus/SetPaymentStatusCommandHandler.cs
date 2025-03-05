using ErrorOr;
using MediatR;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Specs;

namespace TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;

public class SetPaymentStatusCommandHandler
    : IRequestHandler<SetPaymentStatusCommand, ErrorOr<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetPaymentStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Unit>> Handle(SetPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var spec = new OrderByIdSpec(request.OrderId);
        var order = await _unitOfWork.Repository<Order>().FirstOrDefaultAsync(spec, cancellationToken);

        if (order == null)
        {
            return OrderErrors.OrderNotFound(request.OrderId);
        }

        order.SetPaymentStatus(request.IsPaymentSuccessful, request.PaymentIntentId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
