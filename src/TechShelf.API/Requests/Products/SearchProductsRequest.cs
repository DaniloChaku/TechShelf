using System.ComponentModel.DataAnnotations;
using TechShelf.Domain.Products.Enums;

namespace TechShelf.API.Requests.Products;

public record SearchProductsRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Page index must be greater than 0")] int PageIndex,
    [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")] int PageSize,
    int? BrandId = null,
    int? CategoryId = null,
    string? Name = null,
    int? MinPrice = null,
    int? MaxPrice = null,
    ProductsSortOption? SortBy = null);
