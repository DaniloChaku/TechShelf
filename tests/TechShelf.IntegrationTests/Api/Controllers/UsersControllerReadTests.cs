using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TechShelf.API.Common.Http;
using TechShelf.API.Common.Responses;
using TechShelf.API.Requests.Users;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Domain.Common;
using TechShelf.Domain.Users;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class UsersControllerReadTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly JwtTestHelper _jwtHelper;
    private readonly IServiceScope _scope;

    public UsersControllerReadTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _jwtHelper = new JwtTestHelper(_scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    #region Login

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
        problemDetails!.Errors.Should().ContainKey("email");
    }

    #endregion

    #region GetCurrentUser

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
        userDto.FullName.Should().Be(user.FullName);
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

    #endregion

    #region RefreshToken

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

    #endregion

    #region ForgotPassword

    [Fact]
    public async Task ForgotPassword_ReturnsOk_WhenCommandSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ForgotPasswordRequest(CustomerHelper.Customer1.Email!);

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ForgotPassword, request);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ForgotPasswordRequest("");

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ForgotPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("email");
    }

    [Fact]
    public async Task ForgotPassword_ReturnsInternalError_WhenEmailDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "nonexistent@example.com";
        var request = new ForgotPasswordRequest(email);

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ForgotPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Contain(UserErrors.PasswordResetFailed.Description);
    }

    #endregion

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
