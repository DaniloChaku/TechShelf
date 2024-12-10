using MediatR;
using TechShelf.Application.Common;
using TechShelf.Application.Features.Products.Queries.Shared;

namespace TechShelf.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsQuery(
    int PageIndex,
    int PageSize,
    int? BrandId = null,
    int? CategoryId = null,
    string? Name = null,
    int? MinPrice = null,
    int? MaxPrice = null
) : IRequest<PagedResult<ProductDto>>;

