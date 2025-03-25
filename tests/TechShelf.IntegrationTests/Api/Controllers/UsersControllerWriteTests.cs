using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net;
using TechShelf.API.Common.Responses;
using TechShelf.API.Requests.Users;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using TechShelf.IntegrationTests.TestHelpers.TestData;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class UsersControllerWriteTests : BaseWriteIntegrationTest
{
    private IServiceScope _scope = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _scope = Factory.Services.CreateScope();
    }

    public override async Task DisposeAsync()
    {
        _scope?.Dispose();
        await base.DisposeAsync();
    }

    #region RegisterCustomer

    [Fact]
    public async Task RegisterCustomer_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var expectedUser = new RegisterCustomerRequest(
            FullName: "John Doe",
            Email: "johndoe@example.com",
            PhoneNumber: "+12345678901",
            Password: "SecurePassword123!"
        );
        var issuedTime = DateTime.UtcNow;
        var jwtHelper = GetJwtHelper();

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.RegisterCustomer, expectedUser);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        responseData.Should().NotBeNull();
        responseData!.Token.Should().NotBeNullOrEmpty();

        jwtHelper.ValidateJwt(
            responseData.Token, issuedTime, expectedUser.Adapt<ApplicationUser>(), 
            UserRoles.Customer);
        jwtHelper.ValidateRefreshToken(response, issuedTime);
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsProblem_WhenRegistrationFails()
    {
        // Arrange
        var request = new RegisterCustomerRequest(
            FullName: "John Doe",
            Email: "invalid-email",
            PhoneNumber: "+1234567890",
            Password: "SecurePassword123!"
        );

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.RegisterCustomer, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("email");
    }

    #endregion

    #region ChangeFullName

    [Fact]
    public async Task ChangeFullName_ReturnsNoContent_WhenRequestIsValid()
    {
        // Arrange
        var jwtHelper = GetJwtHelper();
        var user = CustomerHelper.Customer1;
        var token = jwtHelper.GenerateToken(user, [UserRoles.Customer]);
        var expectedNewFullName = "New Full Name";
        var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new ChangeFullNameRequest(expectedNewFullName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedUser = await userManager.FindByIdAsync(user.Id);
        updatedUser!.FullName.Should().Be(expectedNewFullName);
    }

    [Fact]
    public async Task ChangeFullName_ReturnsUnauthorized_WhenNoAuthorizationHeader()
    {
        // Arrange
        var request = new ChangeFullNameRequest("New Full Name");

        // Act
        var response = await Client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeFullName_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        // Arrange
        var jwtHelper = GetJwtHelper();
        var user = CustomerHelper.Customer1;
        var token = jwtHelper.GenerateToken(user, [UserRoles.Customer]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new ChangeFullNameRequest("");

        // Act
        var response = await Client.PutAsJsonAsync(ApiUrls.ChangeFullName, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("fullName");
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
        var request = new ResetPasswordRequest(
            Email: user.Email!,
            Token: token,
            Password: newPassword
        );

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

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
        var request = new ResetPasswordRequest(
            Email: "test@example.com",
            Token: "valid-token",
            Password: ""
        );

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

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
        var request = new ResetPasswordRequest(
            Email: CustomerHelper.Customer1.Email!,
            Token: "invalid-reset-token",
            Password: "NewPassword123!"
        );

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPassword_ReturnsInternalError_WhenEmailDoesNotExist()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Email: "nonexistent@example.com",
            Token: "valid-reset-token",
            Password: "NewPassword123!"
        );

        // Act
        var response = await Client.PostAsJsonAsync(ApiUrls.ResetPassword, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
    }

    #endregion

    private JwtTestHelper GetJwtHelper()
    {
        return new(_scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }
}
