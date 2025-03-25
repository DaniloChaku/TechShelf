using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using TechShelf.Application.Common.Pagination;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class OrdersControllerReadTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JwtTestHelper _jwtHelper;
    private readonly IServiceScope _scope;

    public OrdersControllerReadTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _jwtHelper = new JwtTestHelper(_scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    #region GetCustomerOrders

    [Fact]
    public async Task GetCustomerOrders_ReturnsOk_WhenUserIsAuthorized()
    {
        // Arrange
        var superAdmin = AdminHelper.SuperAdmin;
        var pageIndex = 1;
        var pageSize = 10;
        var token = _jwtHelper.GenerateToken(superAdmin, [UserRoles.SuperAdmin]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var customerId = CustomerHelper.Customer1.Id;

        var url = QueryHelpers.AddQueryString($"{ApiUrls.GetCustomerOrders}/{customerId}",
            new Dictionary<string, string?>() {
                {"pageIndex", pageIndex.ToString() },
                {"pageSize", pageSize.ToString() }
            });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEquivalentTo(OrderHelper.Customer1Orders.Adapt<List<OrderDto>>(), options =>
        {
            options.Using<DateTime>(ctx =>
            {
                ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1));
            }).WhenTypeIs<DateTime>();

            return options;
        });
        result.PageIndex.Should().Be(pageIndex);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(OrderHelper.Customer1Orders.Count);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsUnauthorized_WhenNoAuthorizationHeader()
    {
        // Arrange
        var customerId = CustomerHelper.Customer1.Id;
        var url = QueryHelpers.AddQueryString($"{ApiUrls.GetCustomerOrders}/{customerId}",
            new Dictionary<string, string?>() {
                {"pageIndex", "1" },
                {"pageSize", "1" }
            });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsForbidden_WhenUserNotInRequiredRole()
    {
        // Arrange
        var customer = CustomerHelper.Customer1;
        var token = _jwtHelper.GenerateToken(customer, ["Customer"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var customerId = CustomerHelper.Customer1.Id;
        var url = QueryHelpers.AddQueryString($"{ApiUrls.GetCustomerOrders}/{customerId}",
            new Dictionary<string, string?>() {
                {"pageIndex", "1" },
                {"pageSize", "1" }
            });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GetMyOrders
    [Fact]
    public async Task GetMyOrders_ReturnsOk_WhenUserIsAuthorizedAsCustomer()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;
        var customer = CustomerHelper.Customer1;
        var token = _jwtHelper.GenerateToken(customer, [UserRoles.Customer]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = QueryHelpers.AddQueryString(ApiUrls.GetMyOrders, new Dictionary<string, string?>
        {
            { "pageIndex", pageIndex.ToString() },
            { "pageSize", pageSize.ToString() }
        });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEquivalentTo(OrderHelper.Customer1Orders.Adapt<List<OrderDto>>(), options =>
        {
            options.Using<DateTime>(ctx =>
            {
                ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1));
            }).WhenTypeIs<DateTime>();
            return options;
        });
        result.PageIndex.Should().Be(pageIndex);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(OrderHelper.Customer1Orders.Count);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsUnauthorized_WhenNoAuthorizationHeader()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;
        _client.DefaultRequestHeaders.Authorization = null;
        var url = QueryHelpers.AddQueryString(ApiUrls.GetMyOrders, new Dictionary<string, string?>
        {
            { "pageIndex", pageIndex.ToString() },
            { "pageSize", pageSize.ToString() }
        });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsForbidden_WhenUserIsNotInCustomerRole()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;
        var superAdmin = AdminHelper.SuperAdmin;
        var token = _jwtHelper.GenerateToken(superAdmin, [UserRoles.SuperAdmin]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = QueryHelpers.AddQueryString(ApiUrls.GetMyOrders, new Dictionary<string, string?>
        {
            { "pageIndex", pageIndex.ToString() },
            { "pageSize", pageSize.ToString() }
        });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsBadRequest_WhenInvalidPageValuesAreSupplied()
    {
        // Arrange
        var pageIndex = -1;
        var pageSize = -1;
        var superAdmin = AdminHelper.SuperAdmin;
        var token = _jwtHelper.GenerateToken(superAdmin, [UserRoles.Customer]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = QueryHelpers.AddQueryString(ApiUrls.GetMyOrders, new Dictionary<string, string?>
        {
            { "pageIndex", pageIndex.ToString() },
            { "pageSize", pageSize.ToString() }
        });

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
    }

    #endregion

    public void Dispose()
    {
        _scope.Dispose();
    }
}
