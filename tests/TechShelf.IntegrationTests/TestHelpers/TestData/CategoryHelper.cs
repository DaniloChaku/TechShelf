using TechShelf.Application.Features.Categories.Queries.Shared;
using TechShelf.Domain.Products;
using TechShelf.Infrastructure.Data;

namespace TechShelf.IntegrationTests.TestHelpers.TestData;

public static class CategoryHelper
{
    public static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Categories.AddRange(Categories);
        dbContext.SaveChanges();
    }

    public static List<Category> Categories =>
        [
            new Category { Id = 1, Name = "Category X" },
            new Category { Id = 2, Name = "Category Y" }
        ];

    public static bool Equals(CategoryDto categoryDto, Category category)
    {
        return categoryDto.Id == category.Id && categoryDto.Name == category.Name;
    }
}

