using FluentValidation.TestHelper;
using TechShelf.Application.Features.Users.Commands.ForgotPassword;

namespace TechShelf.UnitTests.Application.Features.Users.ForgotPassword;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator;

    public ForgotPasswordCommandValidatorTests()
    {
        _validator = new ForgotPasswordCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("email")]
    public void Validator_HasError_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Arrange
        var command = new ForgotPasswordCommand(invalidEmail!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("valid@example.com")]
    [InlineData("some.customer@gmail.com")]
    public void Validator_HasNoError_WhenEmailIsValid(string validEmail)
    {
        // Arrange
        var command = new ForgotPasswordCommand(validEmail);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
