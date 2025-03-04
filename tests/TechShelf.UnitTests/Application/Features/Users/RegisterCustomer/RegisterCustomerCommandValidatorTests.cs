using FluentValidation.TestHelper;
using TechShelf.Application.Features.Users.Commands.RegisterCustomer;

namespace TechShelf.UnitTests.Application.Features.Users.RegisterCustomer;

public class RegisterCustomerCommandValidatorTests
{
    private readonly RegisterCustomerCommandValidator _validator;

    public RegisterCustomerCommandValidatorTests()
    {
        _validator = new RegisterCustomerCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenFirstNameIsInvalid(string? invalidFirstName)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { FirstName = invalidFirstName! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Jane")]
    [InlineData("William")]
    public void Validator_HasNoError_WhenFirstNameIsValid(string validFirstName)
    {
        // Arrange
        var command = CreateValidCommand() with { FirstName = validFirstName };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validator_HasError_WhenLastNameIsInvalid(string? invalidLastName)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { LastName = invalidLastName! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("Doe")]
    [InlineData("Smith")]
    [InlineData("Ivanov")]
    public void Validator_HasNoError_WhenLastNameIsValid(string validLastName)
    {
        // Arrange
        var command = CreateValidCommand() with { LastName = validLastName };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
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
    [InlineData("911")]
    [InlineData("phoneNumber")]
    public void Validator_HasError_WhenPhoneNumberIsInvalid(string? invalidPhoneNumber)
    {
        // Arrange
        var command = CreateValidCommand()
            with
        { PhoneNumber = invalidPhoneNumber! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+1234567890")]
    public void Validator_HasNoError_WhenPhoneNumberIsValid(string validPhoneNumber)
    {
        // Arrange
        var command = CreateValidCommand() with { PhoneNumber = validPhoneNumber };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
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

    private RegisterCustomerCommand CreateValidCommand()
    {
        return new RegisterCustomerCommand("Name", "LastName", "email@example.com", "+123456789", "Pa$$word123");
    }
}
