using ErrorOr;
using MediatR;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
public record GetCustomerOrdersQuery(
    string CustomerId,
    int PageIndex,
    int PageSize
) : IRequest<ErrorOr<PagedResult<OrderDto>>>;
