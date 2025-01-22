using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TechShelf.API.Common.Requirements;
using TechShelf.Domain.Common;

namespace TechShelf.UnitTests.Api.Common.Requirements;

public class AllowAnonymousAndCustomerTests
{
    private readonly AllowAnonymousAndCustomer _handler;

    public AllowAnonymousAndCustomerTests()
    {
        _handler = new AllowAnonymousAndCustomer();
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(
            [_handler],
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenUserIsAuthenticatedAndInCustomerRole()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
                new Claim(ClaimTypes.Role, UserRoles.Customer)
            ], "mock"));
        var context = new AuthorizationHandlerContext(
            [_handler],
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSucceed_WhenUserIsAuthenticatedAndNotInCustomerRole()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
                new Claim(ClaimTypes.Role, UserRoles.AdminSupport)
            ], "mock"));
        var context = new AuthorizationHandlerContext(
            [_handler],
            user,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }
}
