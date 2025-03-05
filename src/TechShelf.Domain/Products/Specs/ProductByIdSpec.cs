using Ardalis.Specification;

namespace TechShelf.Domain.Products.Specs;

public class ProductByIdSpec : SingleResultSpecification<Product>
{
    public ProductByIdSpec(int productId)
    {
        Query.Where(p => p.Id == productId)
            .Include(p => p.Brand)
            .Include(p => p.Category);
    }
}
