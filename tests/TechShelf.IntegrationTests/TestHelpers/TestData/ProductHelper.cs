using TechShelf.Application.Features.Products.Queries.Shared;
using TechShelf.Domain.Entities;
using TechShelf.Infrastructure.Data;

namespace TechShelf.IntegrationTests.TestHelpers.Seed;

public static class ProductHelper
{
    public static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Products.AddRange(Products);
        dbContext.SaveChanges();
    }

    public static List<Product> Products =>
        [
            new Product
            {
                Id = 1,
                Name = "Product 1",
                Description = "Description of Product 1",
                CategoryId = 1,
                BrandId = 1,
                Price = 99.99m,
                Stock = 50,
                ThumbnailUrl = "product1.jpg",
                ImageUrls = ["product1.jpg", "product1-2.jpg"]
            },
            new Product
            {
                Id = 2,
                Name = "Product 2",
                Description = "Description of Product 2",
                CategoryId = 2,
                BrandId = 2,
                Price = 149.99m,
                Stock = 30,
                ThumbnailUrl = "product2.jpg",
                ImageUrls = ["product2.jpg", "product2-2.jpg"]
            },
            new Product
            {
                Id = 3,
                Name = "Product 3",
                Description = "Description of Product 3",
                CategoryId = 1,
                BrandId = 1,
                Price = 169.99m,
                Stock = 40,
                ThumbnailUrl = "product3.jpg",
                ImageUrls = ["product3.jpg", "product3-2.jpg"]
            }
        ];

    public static bool Equals(ProductDto productDto, Product product, bool compareReferences = false)
    {
        return productDto.Id == product.Id &&
               productDto.Name == product.Name &&
               productDto.Description == product.Description &&
               productDto.Price == product.Price &&
               productDto.CategoryId == product.CategoryId &&
               productDto.BrandId == product.BrandId &&
               productDto.ThumbnailUrl == product.ThumbnailUrl &&
               productDto.ImageUrls.SequenceEqual(product.ImageUrls) && 
               (compareReferences && 
               CategoryHelper.Equals(productDto.Category!, CategoryHelper.Categories.First(c => c.Id == product.CategoryId)) &&
               BrandHelper.Equals(productDto.Brand!, BrandHelper.Brands.First(b => b.Id == product.BrandId)));
    }
}

