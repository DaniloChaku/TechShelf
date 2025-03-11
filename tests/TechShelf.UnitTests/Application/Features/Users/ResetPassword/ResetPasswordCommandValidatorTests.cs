using FluentValidation.TestHelper;
using TechShelf.Application.Features.Users.Commands.ResetPassword;

namespace TechShelf.UnitTests.Application.Features.Users.ResetPassword;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator;

    public ResetPasswordCommandValidatorTests()
    {
        _validator = new();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validator_HasError_WhenTokenIsInvalid(string? token)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { Token = token! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData("token")]
    [InlineData("another-token12345!")]
    public void Validator_HasNoError_WhenTokenIsValid(string validToken)
    {
        // Arrange
        var command = CreateValidCommand() with { Token = validToken };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("email")]
    public void Validator_HasError_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { Email = invalidEmail! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("some.user@gmail.com")]
    public void Validate_HasNoError_WhenEmailIsValid(string validEmail)
    {
        // Arrange
        var command = CreateValidCommand() with { Email = validEmail };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Password123")] // Missing special character
    [InlineData("password123!")] // Missing uppercase letter
    [InlineData("PASSWORD123!")] // Missing lowercase letter
    [InlineData("Password!")]    // Missing digit
    public void Validator_HasError_WhenPasswordIsInvalid(string? invalidPassword)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { Password = invalidPassword! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("P@ssw0rd")]
    [InlineData("Abcd1234!")]
    [InlineData("S0mething@Special")]
    public void Validator_HasNoError_WhenPasswordIsValid(string validPassword)
    {
        // Arrange
        var command = CreateValidCommand() with { Password = validPassword };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    public ResetPasswordCommand CreateValidCommand()
    {
        return new("token", "user@example.com", "Password123!");
    }
}
