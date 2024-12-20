using System.Net.Http.Json;
using System.Net;
using TechShelf.API.Common.Requests.Users;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.API.Common.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TechShelf.IntegrationTests.TestHelpers.Seed;
using System.Net.Http.Headers;
using TechShelf.Application.Features.Users.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TechShelf.Domain.Common;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class UsersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly JwtTestHelper _jwtHelper;

    public UsersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _jwtHelper = new JwtTestHelper(
            factory.Services.GetRequiredService<IConfiguration>(),
            factory.Services.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterCustomerRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "johndoe@example.com",
            PhoneNumber: "+1234567890",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.RegisterCustomer, request);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();
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

        // Act
        var response = await client.PostAsJsonAsync(ApiUrls.Login, loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();
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
        userDto!.FirstName.Should().Be(user.FirstName);
        userDto!.LastName.Should().Be(user.LastName);
        userDto!.Email.Should().Be(user.Email);
        userDto!.PhoneNumber.Should().Be(user.PhoneNumber);
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
}
