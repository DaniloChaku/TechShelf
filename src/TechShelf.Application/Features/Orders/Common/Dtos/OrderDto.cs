namespace TechShelf.Application.Features.Orders.Common.Dtos;

public record OrderDto(
    Guid Id,
    string Email,
    string PhoneNumber,
    string FullName,
    string? CustomerId,
    string? PaymentIntentId,
    decimal Total,
    AddressDto Address,
    List<OrderItemDto> OrderItems,
    List<OrderHistoryEntryDto> History
);
