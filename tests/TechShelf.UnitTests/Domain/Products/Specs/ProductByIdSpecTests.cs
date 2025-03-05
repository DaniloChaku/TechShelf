using FluentAssertions;
using TechShelf.Domain.Products;
using TechShelf.Domain.Products.Specs;

namespace TechShelf.UnitTests.Domain.Products.Specs;

public class ProductByIdSpecTests
{
    [Fact]
    public void MatchesProductById()
    {
        // Arrange
        var product = new Product()
        {
            Id = 1
        };

        var spec = new ProductByIdSpec(product.Id);

        // Act
        var result = spec.IsSatisfiedBy(product);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesNoProductById_WhenIdNotPresent()
    {
        // Arrange
        var product = new Product()
        {
            Id = 1
        };

        var spec = new ProductByIdSpec(-1);

        // Act
        var result = spec.IsSatisfiedBy(product);

        // Assert
        result.Should().BeFalse();
    }
}
