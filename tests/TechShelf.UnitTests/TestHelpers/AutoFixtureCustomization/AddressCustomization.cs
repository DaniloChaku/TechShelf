using AutoFixture;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.UnitTests.TestHelpers.AutoFixtureCustomization;

public class AddressCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Address>(composer => composer
            .FromFactory(() => new Address(
                country: Address.AllowedCountries[0],
                addressLine1: fixture.Create<string>(),
                addressLine2: fixture.Create<string>(),
                city: fixture.Create<string>(),
                region: fixture.Create<string>(),
                postalCode: fixture.Create<string>())));
    }
}
