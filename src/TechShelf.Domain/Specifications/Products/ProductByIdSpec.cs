using Ardalis.Specification;
using TechShelf.Domain.Entities;

namespace TechShelf.Domain.Specifications.Products;

public class ProductByIdSpec : SingleResultSpecification<Product>
{
    public ProductByIdSpec(int productId)
    {
        Query.Where(p => p.Id == productId)
            .Include(p => p.Brand)
            .Include(p => p.Category);
    }
}
