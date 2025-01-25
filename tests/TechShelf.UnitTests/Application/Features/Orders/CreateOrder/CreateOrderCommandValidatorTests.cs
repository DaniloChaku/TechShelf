using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Commands.CreateOrder;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.UnitTests.Application.Features.Orders.CreateOrder;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validator_HasError_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { Email = invalidEmail! };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    public void Validator_HasNoError_WhenEmailIsValid(string validEmail)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { Email = validEmail };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("invalid-phone")]
    [InlineData("+123456789012345678")] 
    public void Validator_HasError_WhenPhoneNumberIsInvalid(string? invalidPhone)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { PhoneNumber = invalidPhone! };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+1234567890123")]
    public void Validator_HasNoError_WhenPhoneNumberIsValid(string validPhone)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { PhoneNumber = validPhone };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_HasError_WhenNameIsInvalid(string? invalidFullName)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { Name = invalidFullName! };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Jane Smith")]
    public void Validator_HasNoError_WhenNameIsValid(string validFullName)
    {
        // Arrange
        var command = CreateValidOrderCommand() with { Name = validFullName };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_HasError_WhenBasketItemsIsEmpty()
    {
        // Arrange
        var command = CreateValidOrderCommand() with { BasketItems = new List<BasketItem>() };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BasketItems);
    }

    [Fact]
    public void Validator_HasError_WhenBasketItemsAreEmpty()
    {
        // Arrange
        var invalidBasketItems = new List<BasketItem>();
        var command = CreateValidOrderCommand() with { BasketItems = invalidBasketItems };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor("BasketItems");
    }

    [Fact]
    public void Validator_HasError_WhenBasketItemsContainInvalidItem()
    {
        // Arrange
        var invalidBasketItems = new List<BasketItem>
       {
           new BasketItem(ProductId: -1, Quantity: 0) 
       };
        var command = CreateValidOrderCommand() with { BasketItems = invalidBasketItems };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldHaveValidationErrorFor("BasketItems[0].ProductId");
        result.ShouldHaveValidationErrorFor("BasketItems[0].Quantity");
    }

    [Fact]
    public void Validator_HasNoError_WhenBasketItemsAreValid()
    {
        // Arrange
        var validBasketItems = new List<BasketItem>
       {
           new BasketItem(ProductId: 1, Quantity: 1),
           new BasketItem(ProductId: 2, Quantity: 3)
       };
        var command = CreateValidOrderCommand() with { BasketItems = validBasketItems };
        // Act
        var result = _validator.TestValidate(command);
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BasketItems);
    }

    private static CreateOrderCommand CreateValidOrderCommand() => new(
        Email: "test@example.com",
        PhoneNumber: "+1234567890",
        Name: "John Doe",
        ShippingAddress: new AddressDto(
            Line1: "123 Main St",
            Line2: null,
            City: "New York",
            State: "NY",
            PostalCode: "10001"
        ),
        BasketItems:
        [
           new BasketItem(ProductId: 1, Quantity: 1)
        ],
        UserId: null
    );
}
