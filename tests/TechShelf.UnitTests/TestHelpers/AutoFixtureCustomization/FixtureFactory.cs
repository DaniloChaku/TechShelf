using AutoFixture;

namespace TechShelf.UnitTests.TestHelpers.AutoFixtureCustomization;

public static class FixtureFactory
{
    public static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize(new AddressCustomization());
        return fixture;
    }
}
