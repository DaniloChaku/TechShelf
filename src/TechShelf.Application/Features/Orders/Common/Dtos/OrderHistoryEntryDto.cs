namespace TechShelf.Application.Features.Orders.Common.Dtos;

public record OrderHistoryEntryDto(
    Guid OrderId,
    string Status,
    DateTime Date,
    string? Notes
);
