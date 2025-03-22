using System.Net.Http.Json;
using FluentAssertions;
using TechShelf.Application.Features.Brands.Queries.Shared;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class BrandsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public BrandsControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ReturnsBrands()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(ApiUrls.Brands);

        // Assert
        response.EnsureSuccessStatusCode();
        var actualBrands = await response.Content.ReadFromJsonAsync<List<BrandDto>>();

        actualBrands.Should().NotBeNull();
        actualBrands.Should().NotBeEmpty();
        actualBrands!.Count.Should().Be(BrandHelper.Brands.Count);
        foreach (var brand in BrandHelper.Brands)
        {
            actualBrands.Should().ContainSingle(b => BrandHelper.Equals(b, brand));
        }
    }
}
