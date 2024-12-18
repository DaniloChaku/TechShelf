using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TechShelf.API.Common.Requests.Products;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Products.Queries.Shared;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.Seed;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class ProductsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(1, 10, null, null, null, null, null)]
    [InlineData(1, 10, 1, null, null, null, null)]
    [InlineData(1, 10, null, 1, null, null, null)]
    [InlineData(1, 10, null, null, "Product X", null, null)]
    [InlineData(1, 10, null, null, null, 100, null)]
    [InlineData(1, 10, null, null, null, null, 200)]
    [InlineData(1, 10, 1, 1, "Product X", 100, 200)]
    public async Task SearchProducts_ReturnsExpectedResults(
            int pageIndex, int pageSize, int? brandId, int? categoryId,
            string? name, int? minPrice, int? maxPrice)
    {
        // Arrange
        var queryParams = new Dictionary<string, string?>
        {
            { nameof(SearchProductsRequest.PageIndex), pageIndex.ToString() },
            { nameof(SearchProductsRequest.PageSize), pageSize.ToString() },
            { nameof(SearchProductsRequest.BrandId), brandId?.ToString() },
            { nameof(SearchProductsRequest.CategoryId), categoryId?.ToString() },
            { nameof(SearchProductsRequest.Name), name },
            { nameof(SearchProductsRequest.MinPrice), minPrice?.ToString() },
            { nameof(SearchProductsRequest.MaxPrice), maxPrice?.ToString() }
        };

        var expectedProducts = ProductHelper.Products
            .Where(p =>
                (!brandId.HasValue || p.BrandId == brandId) &&
                (!categoryId.HasValue || p.CategoryId == categoryId) &&
                (string.IsNullOrEmpty(name) || p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                (!minPrice.HasValue || p.Price >= minPrice) &&
                (!maxPrice.HasValue || p.Price <= maxPrice))
            .ToList();

        var queryString = string.Join("&", queryParams
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var url = $"{ApiUrls.ProductsSearch}/?{queryString}";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(expectedProducts.Count);
        foreach (var product in expectedProducts)
        {
            result.Items.Should().ContainSingle(p => ProductHelper.Equals(p, product, true));
        }
    }

    [Fact]
    public async Task SearchProducts_ReturnsValidationError_WhenRequestIsInvalid()
    {
        // Arrange
        var invalidRequest = new SearchProductsRequest(PageIndex: 0, PageSize: -1);

        var queryString = $"?PageIndex={invalidRequest.PageIndex}&PageSize={invalidRequest.PageSize}";

        // Act
        var response = await _client.GetAsync(ApiUrls.ProductsSearch + queryString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey(nameof(SearchProductsRequest.PageIndex));
        problemDetails.Errors.Should().ContainKey(nameof(SearchProductsRequest.PageSize));
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var expectedProduct = ProductHelper.Products[0];

        // Act
        var response = await _client.GetAsync($"{ApiUrls.Products}/{expectedProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();

        product.Should().NotBeNull();
        ProductHelper.Equals(product!, expectedProduct, true).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenIdIsInvalid()
    {
        // Arrange
        var nonExistentProductId = int.MaxValue;

        // Act
        var response = await _client.GetAsync($"{ApiUrls.Products}/{nonExistentProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
}
