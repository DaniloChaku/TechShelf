using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechShelf.API.Common.Http;
using TechShelf.API.Common.Requests.Users;
using TechShelf.API.Common.Responses;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class UsersControllerTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly JwtTestHelper _jwtHelper;
    private readonly IServiceScope _scope;

    public UsersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _jwtHelper = new JwtTestHelper(_scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var expectedUser = new RegisterCustomerRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "johndoe@example.com",
            PhoneNumber: "+1234567890",
            Password: "SecurePassword123!"
        );
        var issuedTime = DateTime.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.RegisterCustomer, expectedUser);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();

        _jwtHelper.ValidateJwt(responseData.Token, issuedTime, expectedUser.Adapt<ApplicationUser>(), UserRoles.Customer);
        _jwtHelper.ValidateRefreshToken(response, issuedTime);
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsProblem_WhenRegistrationFails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterCustomerRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "invalid-email",
            PhoneNumber: "+1234567890",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.RegisterCustomer, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest(
            Email: AdminHelper.SuperAdminOptions.Email,
            Password: AdminHelper.SuperAdminOptions.Password
        );
        var issuedTime = DateTime.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.Login, loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();

        _jwtHelper.ValidateJwt(responseData.Token, issuedTime, AdminHelper.SuperAdmin, UserRoles.SuperAdmin);
        _jwtHelper.ValidateRefreshToken(response, issuedTime);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();

        var loginRequest = new LoginRequest(
            Email: AdminHelper.SuperAdminOptions.Email,
            Password: "WrongPassword123!"
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.Login, loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ReturnsProblem_WhenEmailFormatIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest(
            Email: "invalid-email",
            Password: AdminHelper.SuperAdminOptions.Password
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.Login, loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOk_WhenUserIsAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = AdminHelper.SuperAdmin;
        var token = _jwtHelper.GenerateToken(user, [UserRoles.SuperAdmin]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync(ApiUrls.Me);

        // Assert
        response.EnsureSuccessStatusCode();
        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Id.Should().NotBeNull();
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.Email.Should().Be(user.Email);
        userDto.PhoneNumber.Should().Be(user.PhoneNumber);
        userDto.Roles.Should().BeEquivalentTo(UserRoles.SuperAdmin);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenNoAuthorizationHeader()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(ApiUrls.Me);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync(ApiUrls.Me);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenRefreshTokenIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = AdminHelper.SuperAdmin;

        var refreshToken = await _jwtHelper.GenerateRefreshToken(user.Email!);
        var accessToken = _jwtHelper.GenerateToken(user, [UserRoles.SuperAdmin]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Add("Cookie", $"{Cookies.RefreshToken}={refreshToken}");
        var issuedTime = DateTime.UtcNow;

        // Act
        var response = await client.PostAsync(ApiUrls.RefreshToken, null);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();

        _jwtHelper.ValidateJwt(responseData.Token, issuedTime, user, UserRoles.SuperAdmin);
        _jwtHelper.ValidateRefreshToken(response, issuedTime);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenNoRefreshTokenCookie()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = AdminHelper.SuperAdmin;
        var accessToken = _jwtHelper.GenerateToken(user, [UserRoles.SuperAdmin]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.PostAsync(ApiUrls.RefreshToken, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = AdminHelper.SuperAdmin;
        var accessToken = _jwtHelper.GenerateToken(user, [UserRoles.SuperAdmin]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Add("Cookie", $"{Cookies.RefreshToken}=invalid-refresh-token");

        // Act
        var response = await client.PostAsync(ApiUrls.RefreshToken, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenJwtTokenIsMissingEmailClaim()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = AdminHelper.SuperAdmin;

        var refreshToken = await _jwtHelper.GenerateRefreshToken(user.Email!);

        var invalidJwtToken = _jwtHelper.GenerateToken(user, [UserRoles.SuperAdmin], false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalidJwtToken);
        client.DefaultRequestHeaders.Add("Cookie", $"{Cookies.RefreshToken}={refreshToken}");

        // Act
        var response = await client.PostAsync(ApiUrls.RefreshToken, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
