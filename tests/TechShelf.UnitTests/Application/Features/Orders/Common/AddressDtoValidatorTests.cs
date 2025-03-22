using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Common.Validators;
using TechShelf.Domain.Orders;

namespace TechShelf.UnitTests.Application.Features.Orders.Common;

public class AddressDtoValidatorTests
{
    private readonly AddressDtoValidator _validator;

    public AddressDtoValidatorTests()
    {
        _validator = new AddressDtoValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenLine1IsInvalid(string? invalidLine1)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Line1 = invalidLine1! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Line1);
    }

    [Theory]
    [InlineData("123 Main Street")]
    [InlineData("412, Other Street")]
    public void Validator_HasNoError_WhenLine1IsValid(string validLine1)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Line1 = validLine1 };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Line1);
    }

    [Fact]
    public void Validator_HasError_WhenLine2IsInvalid()
    {
        // Arrange
        var invalidLine2 = "x".PadLeft(OrderConstants.Address.Line2MaxLength + 1);
        var addressDto = CreateValidAddressDto() with { Line2 = invalidLine2 };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Line2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("412, Other Street")]
    public void Validator_HasNoError_WhenLine2IsValid(string? validLine2)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Line2 = validLine2 };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Line2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenCityIsInvalid(string? invalidCity)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { City = invalidCity! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Theory]
    [InlineData("New York")]
    public void Validator_HasNoError_WhenCityIsValid(string validCity)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { City = validCity };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenStateIsInvalid(string? invalidState)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { State = invalidState! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Theory]
    [InlineData("NY")]
    public void Validator_HasNoError_WhenStateIsValid(string validState)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { State = validState };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenPostalCodeIsInvalid(string? invalidPostalCode)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { PostalCode = invalidPostalCode! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }

    [Theory]
    [InlineData("10001")]
    public void Validator_HasNoError_WhenPostalCodeIsValid(string validPostalCode)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { PostalCode = validPostalCode };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PostalCode);
    }

    private AddressDto CreateValidAddressDto()
    {
        return new AddressDto(
            Line1: "123 Main Street",
            Line2: null,
            City: "New York",
            State: "NY",
            PostalCode: "10001");
    }
}
