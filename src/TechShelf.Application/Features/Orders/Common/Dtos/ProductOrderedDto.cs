namespace TechShelf.Application.Features.Orders.Common.Dtos;

public record ProductOrderedDto(
    int ProductId,
    string Name,
    string ImageUrl);
