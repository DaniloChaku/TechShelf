using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Application.Features.Orders.Common.Validators;

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
    public void Validator_HasError_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Name = invalidName! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Jane Smith")]
    public void Validator_HasNoError_WhenNameIsValid(string validName)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Name = validName };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("XX")]
    public void Validator_HasError_WhenCountryIsInvalid(string? invalidCountry)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Country = invalidCountry! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Theory]
    [InlineData("US")]
    public void Validator_HasNoError_WhenCountryIsValid(string validCountry)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Country = validCountry };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Country);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenAddressLine1IsInvalid(string? invalidAddressLine1)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { AddressLine1 = invalidAddressLine1! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AddressLine1);
    }

    [Theory]
    [InlineData("123 Main Street")]
    public void Validator_HasNoError_WhenAddressLine1IsValid(string validAddressLine1)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { AddressLine1 = validAddressLine1 };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AddressLine1);
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
    public void Validator_HasError_WhenRegionIsInvalid(string? invalidRegion)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Region = invalidRegion! };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Region);
    }

    [Theory]
    [InlineData("NY")]
    public void Validator_HasNoError_WhenRegionIsValid(string validRegion)
    {
        // Arrange
        var addressDto = CreateValidAddressDto() with { Region = validRegion };

        // Act
        var result = _validator.TestValidate(addressDto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Region);
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
            Name: "John Doe",
            Country: "US",
            AddressLine1: "123 Main Street",
            AddressLine2: null,
            City: "New York",
            Region: "NY",
            PostalCode: "10001");
    }
}
