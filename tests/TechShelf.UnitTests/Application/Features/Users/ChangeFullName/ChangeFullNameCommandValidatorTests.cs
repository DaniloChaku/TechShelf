using FluentValidation.TestHelper;
using TechShelf.Application.Features.Users.Commands.ChangeFullName;

namespace TechShelf.UnitTests.Application.Features.Users.ChangeFullName;

public class ChangeFullNameCommandValidatorTests
{
    private readonly ChangeFullNameCommandValidator _validator;

    public ChangeFullNameCommandValidatorTests()
    {
        _validator = new ChangeFullNameCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_HasError_WhenUserIdIsInvalid(string? invalidUserId)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { UserId = invalidUserId! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("userId")]
    [InlineData("1234567890")]
    [InlineData("1")]
    public void Validator_HasNoError_WhenUserIdIsValid(string userId)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            UserId = userId
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validator_HasError_WhenFullNameIsInvalid(string? invalidFullName)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { FullName = invalidFullName! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("FullName")]
    [InlineData("John Doe")]
    [InlineData("Sam")]
    public void Validator_HasNoError_WhenFullNameIsValid(string fullName)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            FullName = fullName
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    private ChangeFullNameCommand CreateValidCommand() =>
        new(UserId: "userId", FullName: "FullName");
}
