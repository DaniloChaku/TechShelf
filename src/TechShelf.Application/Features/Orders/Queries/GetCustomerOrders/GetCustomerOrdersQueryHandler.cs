using ErrorOr;
using Mapster;
using MediatR;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Orders;
using TechShelf.Domain.Orders.Specs;

namespace TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;

public class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, ErrorOr<PagedResult<OrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<PagedResult<OrderDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageResult = PaginationHelper.CalculatePagination(request.PageIndex, request.PageSize);

        if (pageResult.IsError)
        {
            return pageResult.Errors;
        }

        var (skip, take) = pageResult.Value;

        var spec = new GetCustomerOrdersSpec(request.CustomerId, skip, take);

        var (orders, totalCount) = await _unitOfWork.Repository<Order>().ListWithTotalCountAsync(spec, cancellationToken);

        var orderDtos = orders.Adapt<List<OrderDto>>();

        return new PagedResult<OrderDto>(orderDtos, totalCount, request.PageIndex, request.PageSize);
    }
}
