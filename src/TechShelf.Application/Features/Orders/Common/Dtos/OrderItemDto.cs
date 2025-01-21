namespace TechShelf.Application.Features.Orders.Common.Dtos;

public record OrderItemDto(
    Guid Id,
    decimal Price,
    int Quantity,
    ProductOrderedDto ProductOrdered
);
