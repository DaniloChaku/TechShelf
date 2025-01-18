using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Common.Validators;

namespace TechShelf.UnitTests.Application.Features.Orders.Common;

public class ProductOrderedDtoValidatorTests
{
    private readonly ProductOrderedDtoValidator _validator;

    public ProductOrderedDtoValidatorTests()
    {
        _validator = new ProductOrderedDtoValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_HasError_WhenProductIdIsInvalid(int invalidId)
    {
        // Arrange
        var productDto = CreateValidProductOrderedDto() with { ProductId = invalidId };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Validator_HasNoError_WhenProductIdIsValid(int validId)
    {
        // Arrange
        var productDto = CreateValidProductOrderedDto() with { ProductId = validId };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var productDto = CreateValidProductOrderedDto() with { Name = invalidName! };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_HasError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var invalidName = "".PadRight(201, 'x');
        var productDto = CreateValidProductOrderedDto() with { Name = invalidName };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_HasNoError_WhenNameIsValid()
    {
        // Arrange
        var validName = "".PadRight(200, 'x');
        var productDto = CreateValidProductOrderedDto() with { Name = validName };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_HasError_WhenImageUrlExceedsMaxLength()
    {
        // Arrange
        var invalidUrl = "".PadRight(251, 'x');
        var productDto = CreateValidProductOrderedDto() with { ImageUrl = invalidUrl };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Validator_HasNoError_WhenImageUrlDoesNotExceedMaxLength()
    {
        // Arrange
        var validUrl = "".PadRight(250, 'x');
        var productDto = CreateValidProductOrderedDto() with { ImageUrl = validUrl };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com/image.jpg")]
    public void Validator_HasNoError_WhenImageUrlIsValid(string? validUrl)
    {
        // Arrange
        var productDto = CreateValidProductOrderedDto() with { ImageUrl = validUrl! };
        // Act
        var result = _validator.TestValidate(productDto);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    private static ProductOrderedDto CreateValidProductOrderedDto() => new(
        ProductId: 1,
        Name: "Test Product",
        ImageUrl: "https://example.com/test.jpg"
    );
}
