using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string Email,
    string PhoneNumber,
    string FullName,
    AddressDto ShippingAddress,
    IEnumerable<BasketItem> BasketItems,
    string? UserId = null)
    : IRequest<ErrorOr<Guid>>;
