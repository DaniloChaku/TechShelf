using AutoFixture;
using FluentAssertions;
using Mapster;
using TechShelf.Application.Common.Mappings;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.Application.Common.Mappings;

public class OrderMappingsTests
{
    private readonly IFixture _fixture;

    public OrderMappingsTests()
    {
        _fixture = new Fixture();
        CustomizeFixture();
        OrderMappings.Configure();
    }

    private void CustomizeFixture()
    {
        // Address customization
        _fixture.Customize<Address>(composer => composer
            .FromFactory(() => new Address(
                country: Address.AllowedCountries[0],
                addressLine1: _fixture.Create<string>(),
                addressLine2: _fixture.Create<string>(),
                city: _fixture.Create<string>(),
                region: _fixture.Create<string>(),
                postalCode: _fixture.Create<string>())));
    }

    [Fact]
    public void AdaptAdressToAddressDto_MapsPropertiesCorrectly()
    {
        var address = _fixture.Create<Address>();

        // Act
        var addressDto = address.Adapt<AddressDto>();

        // Assert
        addressDto.Should().NotBeNull();
        addressDto.Country.Should().Be(address.Country);
        addressDto.Line1.Should().Be(address.AddressLine1);
        addressDto.Line2.Should().Be(address.AddressLine2);
        addressDto.City.Should().Be(address.City);
        addressDto.State.Should().Be(address.Region);
        addressDto.PostalCode.Should().Be(address.PostalCode);
    }

    [Fact]
    public void AdaptAdressToAddressDto_MapsPropertiesCorrectly_WhenAddressLine2IsNull()
    {
        var address = _fixture.Build<Address>()
           .FromFactory(() => new Address(
               country: Address.AllowedCountries[0],
               addressLine1: _fixture.Create<string>(),
               addressLine2: null,
               city: _fixture.Create<string>(),
               region: _fixture.Create<string>(),
               postalCode: _fixture.Create<string>()))
           .Create();

        // Act
        var addressDto = address.Adapt<AddressDto>();

        // Assert
        addressDto.Should().NotBeNull();
        addressDto.Line2.Should().BeNull();
        addressDto.Country.Should().Be(address.Country);
        addressDto.Line1.Should().Be(address.AddressLine1);
        addressDto.City.Should().Be(address.City);
        addressDto.State.Should().Be(address.Region);
        addressDto.PostalCode.Should().Be(address.PostalCode);
    }

    [Fact]
    public void AdaptOrderToOrderDto_MapsPropertiesCorrectly()
    {
        // Arrange
        var order = _fixture.Create<Order>();

        // Act
        var orderDto = order.Adapt<OrderDto>();

        // Assert
        orderDto.Should().NotBeNull();
        orderDto.Id.Should().Be(order.Id);
        orderDto.Email.Should().Be(order.Email);
        orderDto.PhoneNumber.Should().Be(order.PhoneNumber);
        orderDto.FullName.Should().Be(order.FullName);
        orderDto.Total.Should().Be(order.Total);

        // Assert Address mapping
        orderDto.Address.Should().NotBeNull();
        orderDto.Address.Country.Should().Be(order.Address.Country);
        orderDto.Address.Line1.Should().Be(order.Address.AddressLine1);
        orderDto.Address.Line2.Should().Be(order.Address.AddressLine2);
        orderDto.Address.City.Should().Be(order.Address.City);
        orderDto.Address.State.Should().Be(order.Address.Region);
        orderDto.Address.PostalCode.Should().Be(order.Address.PostalCode);

        // Assert OrderItems mapping
        orderDto.OrderItems.Should().NotBeNull();
        orderDto.OrderItems.Should().HaveCount(order.OrderItems.Count);

        for (int i = 0; i < order.OrderItems.Count; i++)
        {
            var originalItem = order.OrderItems[i];
            var mappedItem = orderDto.OrderItems[i];

            mappedItem.ProductOrdered.Name.Should().Be(originalItem.ProductOrdered.Name);
            mappedItem.ProductOrdered.ProductId.Should().Be(originalItem.ProductOrdered.ProductId);
            mappedItem.ProductOrdered.ImageUrl.Should().Be(originalItem.ProductOrdered.ImageUrl);
            mappedItem.Quantity.Should().Be(originalItem.Quantity);
            mappedItem.Price.Should().Be(originalItem.Price);
        }
    }
}
