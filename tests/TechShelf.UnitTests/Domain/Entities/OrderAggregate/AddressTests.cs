using FluentAssertions;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.Domain.Entities.OrderAggregate;

public class AddressTests
{
    [Fact]
    public void InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var country = ValidCountry;
        var addressLine1 = "123 Main Street";
        var addressLine2 = "Apt 4B";
        var city = "New York";
        var region = "NY";
        var postalCode = "10001";

        // Act
        var address = new Address(country, addressLine1, addressLine2, city, region, postalCode);

        // Assert
        address.Country.Should().Be(country);
        address.AddressLine1.Should().Be(addressLine1);
        address.AddressLine2.Should().Be(addressLine2);
        address.City.Should().Be(city);
        address.Region.Should().Be(region);
        address.PostalCode.Should().Be(postalCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("DE")]
    public void ThrowsArgumentException_WhenCountryIsInvalid(string? invalidCountry)
    {
        // Arrange
        var addressLine1 = "123 Main Street";
        var city = "New York";
        var region = "NY";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(invalidCountry!, addressLine1, null, city, region, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*country*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenAddressLine1IsInvalid(string? invalidAddressLine1)
    {
        // Arrange
        var country = ValidCountry;
        var city = "New York";
        var region = "NY";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(country, invalidAddressLine1!, null, city, region, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*addressLine1*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenCityIsInvalid(string? invalidCity)
    {
        // Arrange
        var country = ValidCountry;
        var addressLine1 = "123 Main Street";
        var region = "NY";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(country, addressLine1, null, invalidCity!, region, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*city*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenRegionIsInvalid(string? invalidRegion)
    {
        // Arrange
        var country = ValidCountry;
        var addressLine1 = "123 Main Street";
        var city = "New York";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(country, addressLine1, null, city, invalidRegion!, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*region*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenPostalCodeIsInvalid(string? invalidPostalCode)
    {
        // Arrange
        var country = ValidCountry;
        var addressLine1 = "123 Main Street";
        var city = "New York";
        var region = "NY";

        // Act
        Action act = () => new Address(country, addressLine1, null, city, region, invalidPostalCode!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*postalCode*");
    }

    [Fact]
    public void AddressesAreEqual_WhenAllPropertiesMatch()
    {
        // Arrange
        var address1 = new Address(ValidCountry, "123 Main Street", "Apt 4B", "New York", "NY", "10001");
        var address2 = new Address(ValidCountry, "123 Main Street", "Apt 4B", "New York", "NY", "10001");

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void AddressesAreNotEqual_WhenPropertiesDiffer()
    {
        // Arrange
        var address1 = new Address(ValidCountry, "123 Main Street", "Apt 4B", "New York", "NY", "10001");
        var address2 = new Address(ValidCountry, "123 Main Street", "Apt 4B", "New York", "NY", "10002");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    private static string ValidCountry { get; } = Address.AllowedCounties[0];
}
