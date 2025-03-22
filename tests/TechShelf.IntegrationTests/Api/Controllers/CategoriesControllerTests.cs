using System.Net.Http.Json;
using FluentAssertions;
using TechShelf.Application.Features.Categories.Queries.Shared;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class CategoriesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CategoriesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ReturnsCategories()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(ApiUrls.Categories);

        // Assert
        response.EnsureSuccessStatusCode();
        var actualCategories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        actualCategories.Should().NotBeNull();
        actualCategories.Should().NotBeEmpty();
        actualCategories!.Count.Should().Be(CategoryHelper.Categories.Count);
        foreach (var category in CategoryHelper.Categories)
        {
            actualCategories.Should().ContainSingle(c => CategoryHelper.Equals(c, category));
        }
    }
}
