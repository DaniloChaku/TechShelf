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

    #region RegisterCustomer

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var expectedUser = new RegisterCustomerRequest(
            FullName: "John Doe",
            Email: "johndoe@example.com",
            PhoneNumber: "+12345678901",
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
            FullName: "John Doe",
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
        problemDetails!.Errors.Should().ContainKey("email");
    }

    #endregion

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

    #region ChangeFullName

    [Fact]
    public async Task ChangeFullName_ReturnsNoContent_WhenRequestIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = CustomerHelper.Customer1;
        var token = _jwtHelper.GenerateToken(user, [UserRoles.Customer]);
        var expectedNewFullName = "New Full Name";

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ChangeFullNameRequest(expectedNewFullName);

        // Act
        var response = await client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var updatedUser = await userManager.FindByIdAsync(user.Id);
        updatedUser!.FullName.Should().Be(expectedNewFullName);
    }

    [Fact]
    public async Task ChangeFullName_ReturnsUnauthorized_WhenNoAuthorizationHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ChangeFullNameRequest("New Full Name");

        // Act
        var response = await client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeFullName_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = CustomerHelper.Customer1;
        var token = _jwtHelper.GenerateToken(user, [UserRoles.Customer]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new ChangeFullNameRequest("");

        // Act
        var response = await client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("fullName");
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

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_ReturnsOk_WhenCommandSucceeds()
    {
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = CustomerHelper.Customer1;
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = "NewPassword123!";

        // Arrange
        var client = _factory.CreateClient();
        var request = new ResetPasswordRequest(
            Email: user.Email!,
            Token: token,
            Password: newPassword
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedUser = await userManager.FindByIdAsync(user.Id);
        var result = await userManager.CheckPasswordAsync(updatedUser!, newPassword);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ResetPasswordRequest(
            Email: "test@example.com",
            Token: "valid-token",
            Password: ""
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("password");
    }

    [Fact]
    public async Task ResetPassword_ReturnsInternalError_WhenTokenIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ResetPasswordRequest(
            Email: CustomerHelper.Customer1.Email!,
            Token: "invalid-reset-token",
            Password: "NewPassword123!"
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPassword_ReturnsInternalError_WhenEmailDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ResetPasswordRequest(
            Email: "nonexistent@example.com",
            Token: "valid-reset-token",
            Password: "NewPassword123!"
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
    }

    #endregion

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
