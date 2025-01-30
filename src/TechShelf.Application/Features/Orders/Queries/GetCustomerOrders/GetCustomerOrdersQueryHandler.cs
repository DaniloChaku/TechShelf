using Mapster;
using MediatR;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities.OrderAggregate;
using TechShelf.Domain.Specifications.Orders;

namespace TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;

public class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetCustomerOrdersSpec(request.CustomerId);

        var orders = await _unitOfWork.Repository<Order>().ListAsync(spec, cancellationToken);

        return orders.Adapt<List<OrderDto>>();
    }
}
