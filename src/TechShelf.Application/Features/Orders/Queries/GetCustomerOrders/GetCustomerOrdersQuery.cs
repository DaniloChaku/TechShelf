using MediatR;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Queries.GetCustomerOrders;
public record GetCustomerOrdersQuery(
    string CustomerId
) : IRequest<List<OrderDto>>;
