namespace TechShelf.Application.Features.Orders.Common.Dtos;

public record AddressDto(
    string Country,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string Region,
    string PostalCode);
