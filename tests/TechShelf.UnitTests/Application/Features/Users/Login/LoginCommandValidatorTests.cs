using FluentValidation.TestHelper;
using TechShelf.Application.Features.Users.Commands.Login;

namespace TechShelf.UnitTests.Application.Features.Users.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("email")]
    [InlineData("email.com")]
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
    [InlineData("email@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("customer/department=shipping@example.com")]
    [InlineData("x@example.com")]
    [InlineData("example-indeed@strange-example.com")]
    public void Validator_HasNoError_WhenEmailIsValid(string validEmail)
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

    private LoginCommand CreateValidCommand()
    {
        return new LoginCommand("email@example.com", "Password123$");
    }
}
