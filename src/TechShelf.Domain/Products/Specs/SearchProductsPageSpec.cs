using Ardalis.Specification;
using TechShelf.Domain.Products.Enums;

namespace TechShelf.Domain.Products.Specs;

public class SearchProductsPageSpec : Specification<Product>
{
    public SearchProductsPageSpec(
        int skip,
        int take,
        int? brandId = null,
        int? categoryId = null,
        string? name = null,
        int? minPrice = null,
        int? maxPrice = null,
        ProductsSortOption? sortBy = null)
    {
        if (brandId.HasValue)
        {
            Query.Where(p => p.BrandId == brandId.Value);
        }

        if (categoryId.HasValue)
        {
            Query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var trimmedName = name.Trim().ToLower();
            Query.Where(p => p.Name.ToLower().Contains(trimmedName));
        }

        if (minPrice.HasValue)
        {
            Query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            Query.Where(p => p.Price <= maxPrice.Value);
        }

        if (sortBy.HasValue)
        {
            Query.ApplySorting(sortBy.Value);
        }

        Query.Include(p => p.Brand)
            .Include(p => p.Category);

        Query.Skip(skip).Take(take);
    }
}
