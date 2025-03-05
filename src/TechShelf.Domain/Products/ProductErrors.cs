using ErrorOr;

namespace TechShelf.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(int productId) =>
        Error.NotFound("Product.NotFound", $"The product with ID {productId} could not be found");
}
