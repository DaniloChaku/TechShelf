using TechShelf.Domain.Common;

namespace TechShelf.Domain.Products;

public class Product : AggregateRoot<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
}
