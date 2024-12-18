using System.Net.Http.Json;
using System.Net;
using TechShelf.API.Common.Requests.Users;
using TechShelf.IntegrationTests.TestHelpers;
using TechShelf.API.Common.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace TechShelf.IntegrationTests.Api.Controllers;

public class UsersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public UsersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
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
}
