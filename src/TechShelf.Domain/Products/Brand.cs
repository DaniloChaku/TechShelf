using TechShelf.Domain.Common;

namespace TechShelf.Domain.Products;

public class Brand : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
