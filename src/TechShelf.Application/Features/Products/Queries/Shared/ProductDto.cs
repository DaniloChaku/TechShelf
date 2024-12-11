using TechShelf.Application.Features.Brands.Queries.Shared;
using TechShelf.Application.Features.Categories.Queries.Shared;

namespace TechShelf.Application.Features.Products.Queries.Shared;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = [];
    public CategoryDto? Category { get; set; }
    public BrandDto? Brand { get; set; }
}
