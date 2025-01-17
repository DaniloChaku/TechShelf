using FluentAssertions;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.Domain.Entities.OrderAggregate;

public class ProductOrderedTests
{
    [Fact]
    public void InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Sample Product";
        var imageUrl = "https://example.com/image.jpg";

        // Act
        var product = new ProductOrdered(productId, name, imageUrl);

        // Assert
        product.ProductId.Should().Be(productId);
        product.Name.Should().Be(name);
        product.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void ThrowsArgumentException_WhenProductIdIsEmpty()
    {
        // Arrange
        var productId = Guid.Empty;
        var name = "Sample Product";
        var imageUrl = "https://example.com/image.jpg";

        // Act
        Action act = () => new ProductOrdered(productId, name, imageUrl);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Product ID is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var imageUrl = "https://example.com/image.jpg";

        // Act
        Action act = () => new ProductOrdered(productId, invalidName, imageUrl);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenImageUrlIsInvalid(string invalidImageUrl)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Sample Product";

        // Act
        Action act = () => new ProductOrdered(productId, name, invalidImageUrl);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*imageUrl*");
    }

    [Fact]
    public void ProductsAreEqual_WhenAllPropertiesMatch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Sample Product";
        var imageUrl = "https://example.com/image.jpg";

        var product1 = new ProductOrdered(productId, name, imageUrl);
        var product2 = new ProductOrdered(productId, name, imageUrl);

        // Act & Assert
        product1.Should().Be(product2);
    }

    [Fact]
    public void ProductsAreNotEqual_WhenPropertiesDiffer()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Sample Product";
        var imageUrl = "https://example.com/image.jpg";

        var product1 = new ProductOrdered(productId, name, imageUrl);
        var product2 = new ProductOrdered(Guid.NewGuid(), name, imageUrl);

        // Act & Assert
        product1.Should().NotBe(product2);
    }

    [Fact]
    public void HashCodesAreSame_WhenProductsAreEqual()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Sample Product";
        var imageUrl = "https://example.com/image.jpg";

        var product1 = new ProductOrdered(productId, name, imageUrl);
        var product2 = new ProductOrdered(productId, name, imageUrl);

        // Act
        var hash1 = product1.GetHashCode();
        var hash2 = product2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashCodesAreDifferent_WhenProductsAreNotEqual()
    {
        // Arrange
        var product1 = new ProductOrdered(Guid.NewGuid(), "Product A", "https://example.com/image1.jpg");
        var product2 = new ProductOrdered(Guid.NewGuid(), "Product B", "https://example.com/image2.jpg");

        // Act
        var hash1 = product1.GetHashCode();
        var hash2 = product2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
