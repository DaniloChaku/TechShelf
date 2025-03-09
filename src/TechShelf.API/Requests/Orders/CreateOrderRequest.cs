using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.API.Requests.Orders;

public record CreateOrderRequest(
    string Email,
    string PhoneNumber,
    string Name,
    AddressDto ShippingAddress,
    IEnumerable<ShoppingCartItem> ShoppingCartItems);
