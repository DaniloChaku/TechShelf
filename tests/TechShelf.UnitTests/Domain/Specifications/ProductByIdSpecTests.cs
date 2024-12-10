using FluentAssertions;
using TechShelf.Domain.Entities;
using TechShelf.Domain.Specifications.Products;

namespace TechShelf.UnitTests.Domain.Specifications;

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
