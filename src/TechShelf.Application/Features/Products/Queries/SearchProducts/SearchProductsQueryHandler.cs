using Mapster;
using MediatR;
using TechShelf.Application.Common;
using TechShelf.Application.Features.Products.Queries.Shared;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Specifications.Products;

namespace TechShelf.Application.Features.Products.Queries.SearchProducts;

public class SearchProductsQueryHandler
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PaginationHelper.CalculatePagination(request.PageIndex, request.PageSize);

        var spec = new SearchProductsPageSpec(
            skip: skip,
            take: take,
            brandId: request.BrandId,
            categoryId: request.CategoryId,
            name: request.Name,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice);

        var (products, totalCount) = 
            await _unitOfWork.Repository<Product>().ListWithTotalCountAsync(spec, cancellationToken);

        var productDtos = products.Adapt<List<ProductDto>>();

        return new PagedResult<ProductDto>(productDtos, totalCount, request.PageIndex, request.PageSize);
    }
}
