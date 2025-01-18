using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Common.Validators;

namespace TechShelf.UnitTests.Application.Features.Orders.Common;

public class BasketItemValidatorTests
{
    private readonly BasketItemValidator _validator;

    public BasketItemValidatorTests()
    {
        _validator = new BasketItemValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_HasError_WhenProductIdIsInvalid(int invalidId)
    {
        // Arrange
        var basketItem = CreateValidBasketItem() with { ProductId = invalidId };
        // Act
        var result = _validator.TestValidate(basketItem);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Validator_HasNoError_WhenProductIdIsValid(int validId)
    {
        // Arrange
        var basketItem = CreateValidBasketItem() with { ProductId = validId };
        // Act
        var result = _validator.TestValidate(basketItem);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validator_HasError_WhenQuantityIsInvalid(int invalidQuantity)
    {
        // Arrange
        var basketItem = CreateValidBasketItem() with { Quantity = invalidQuantity };
        // Act
        var result = _validator.TestValidate(basketItem);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validator_HasNoError_WhenQuantityIsValid(int validQuantity)
    {
        // Arrange
        var basketItem = CreateValidBasketItem() with { Quantity = validQuantity };
        // Act
        var result = _validator.TestValidate(basketItem);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    private static BasketItem CreateValidBasketItem() => new(
        ProductId: 1,
        Quantity: 1
    );
}
