using TechShelf.Domain.Common;

namespace TechShelf.Domain.Products;

public class Category : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
