using TechShelf.Domain.Orders.ValueObjects;

namespace TechShelf.UnitTests.Domain.Orders;

public class AddressTests
{
    [Fact]
    public void InitializesProperties_WhenValidParametersProvided()
    {
        // Arrange
        var line1 = "123 Main Street";
        var line2 = "Apt 4B";
        var city = "New York";
        var state = "NY";
        var postalCode = "10001";

        // Act
        var address = new Address(line1, line2, city, state, postalCode);

        // Assert
        address.Line1.Should().Be(line1);
        address.Line2.Should().Be(line2);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.PostalCode.Should().Be(postalCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenLine1IsInvalid(string? invalidLine1)
    {
        // Arrange
        var city = "New York";
        var state = "NY";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(invalidLine1!, null, city, state, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*line1*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenCityIsInvalid(string? invalidCity)
    {
        // Arrange
        var line1 = "123 Main Street";
        var state = "NY";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(line1, null, invalidCity!, state, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*city*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenStateIsInvalid(string? invalidState)
    {
        // Arrange
        var line1 = "123 Main Street";
        var city = "New York";
        var postalCode = "10001";

        // Act
        Action act = () => new Address(line1, null, city, invalidState!, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*state*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentException_WhenPostalCodeIsInvalid(string? invalidPostalCode)
    {
        // Arrange
        var line1 = "123 Main Street";
        var city = "New York";
        var state = "NY";

        // Act
        Action act = () => new Address(line1, null, city, state, invalidPostalCode!);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*postalCode*");
    }

    [Fact]
    public void AddressesAreEqual_WhenAllPropertiesMatch()
    {
        // Arrange
        var address1 = new Address("123 Main Street", "Apt 4B", "New York", "NY", "10001");
        var address2 = new Address("123 Main Street", "Apt 4B", "New York", "NY", "10001");

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void AddressesAreNotEqual_WhenPropertiesDiffer()
    {
        // Arrange
        var address1 = new Address("123 Main Street", "Apt 4B", "New York", "NY", "10001");
        var address2 = new Address("123 Main Street", "Apt 4B", "New York", "NY", "10002");

        // Act & Assert
        address1.Should().NotBe(address2);
    }
}
