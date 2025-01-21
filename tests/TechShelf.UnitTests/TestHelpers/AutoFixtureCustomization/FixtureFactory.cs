using AutoFixture;

namespace TechShelf.UnitTests.TestHelpers.AutoFixtureCustomization;

public static class FixtureFactory
{
    public static IFixture CreateFixture()
    {
        return new Fixture().Customize(new AddressCustomization());
    }
}
