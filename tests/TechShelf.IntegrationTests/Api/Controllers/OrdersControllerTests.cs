using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;
using FluentAssertions;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Mapster;
using TechShelf.Application.Common.Pagination;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class OrdersControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JwtTestHelper _jwtHelper;
    private readonly IServiceScope _scope;

    public OrdersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _jwtHelper = new JwtTestHelper(_scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

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
        // Use a customer user who is not authorized for this endpoint.
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

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
#pragma warning disable S1066 // Mergeable "if" statements should be combined
            if (_scope != null)
            {
                _scope.Dispose();
            }
#pragma warning restore S1066 // Mergeable "if" statements should be combined
        }

        _disposed = true;
    }
}
