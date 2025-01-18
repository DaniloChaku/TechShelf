using MediatR;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string Email,
    string PhoneNumber,
    AddressDto Address,
    IEnumerable<BasketItem> BasketItems,
    string? userId = null)
    : IRequest<Guid>;
