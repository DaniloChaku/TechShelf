using ErrorOr;
using MediatR;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Products.Queries.Shared;
using TechShelf.Domain.Products.Enums;

namespace TechShelf.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsQuery(
    int PageIndex,
    int PageSize,
    int? BrandId = null,
    int? CategoryId = null,
    string? Name = null,
    int? MinPrice = null,
    int? MaxPrice = null,
    ProductsSortOption? SortBy = null
) : IRequest<ErrorOr<PagedResult<ProductDto>>>;

