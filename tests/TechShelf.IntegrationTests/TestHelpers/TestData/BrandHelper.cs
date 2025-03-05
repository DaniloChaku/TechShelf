using TechShelf.Application.Features.Brands.Queries.Shared;
using TechShelf.Domain.Products;
using TechShelf.Infrastructure.Data;

namespace TechShelf.IntegrationTests.TestHelpers.TestData;

public static class BrandHelper
{
    public static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Brands.AddRange(Brands);
        dbContext.SaveChanges();
    }

    public static List<Brand> Brands =>
        [
            new Brand { Id = 1, Name = "Brand A" },
            new Brand { Id = 2, Name = "Brand B" }
        ];

    public static bool Equals(BrandDto brandDto, Brand brand)
    {
        return brandDto.Id == brand.Id && brandDto.Name == brand.Name;
    }
}

