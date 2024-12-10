using FluentAssertions;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Specifications.Products;

namespace TechShelf.UnitTests.Domain.Specifications;

public class SearchProductsPageSpecTests
{
    private readonly List<Product> _testProducts =
    [
        new Product { Id = 1, Name = "Laptop", Price = 1000, BrandId = 1, CategoryId = 1 },
        new Product { Id = 2, Name = "Smartphone", Price = 800, BrandId = 2, CategoryId = 2 },
        new Product { Id = 3, Name = "Tablet", Price = 500, BrandId = 1, CategoryId = 2 }
    ];

    public static IEnumerable<object[]> PaginationTestCases()
    {
        yield return new object[] { 0, 1, new List<int> { 1 } };
        yield return new object[] { 1, 1, new List<int> { 2 } };
        yield return new object[] { 2, 2, new List<int> { 3 } };
        yield return new object[] { 0, 3, new List<int> { 1, 2, 3 } };
        yield return new object[] { 3, 1, new List<int>() };
    }

    [Theory]
    [MemberData(nameof(PaginationTestCases))]
    public void AppliesPagination(int skip, int take, List<int> expectedIds)
    {
        // Arrange
        var spec = new SearchProductsPageSpec(skip, take);

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Select(p => p.Id).Should().Equal(expectedIds);
    }

    [Fact]
    public void MatchesProductsByBrandId()
    {
        var spec = new SearchProductsPageSpec(0, 10, brandId: 1);

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.BrandId == 1).Should().BeTrue();
    }

    [Fact]
    public void MatchesProductsByCategoryId()
    {
        var spec = new SearchProductsPageSpec(0, 10, categoryId: 1);

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Should().HaveCount(1);
        result.All(p => p.BrandId == 1).Should().BeTrue();
    }

    [Fact]
    public void MatchesProductsByName()
    {
        var spec = new SearchProductsPageSpec(0, 10, name: "phone");

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Smartphone");
    }

    [Fact]
    public void MatchesProductsByMinPrice()
    {
        var spec = new SearchProductsPageSpec(0, 10, minPrice: 800);

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Price >= 800).Should().BeTrue();
    }

    [Fact]
    public void MatchesProductsByMaxPrice()
    {
        var spec = new SearchProductsPageSpec(0, 10, maxPrice: 800);

        // Act
        var result = spec.Evaluate(_testProducts);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.Price <= 800).Should().BeTrue();
    }
}
