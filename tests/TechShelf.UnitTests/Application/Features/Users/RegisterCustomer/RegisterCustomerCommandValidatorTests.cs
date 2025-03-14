﻿using FluentValidation.TestHelper;
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
    [InlineData("John Smith")]
    [InlineData("Jane Doe")]
    [InlineData("William Black")]
    public void Validator_HasNoError_WhenFirstNameIsValid(string validFullName)
    {
        // Arrange
        var command = CreateValidCommand() with { FullName = validFullName };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
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
    [InlineData("+09987654321")]
    [InlineData("+1234567890")]
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
    [InlineData("+12345678901")]
    [InlineData("+10123456789")]
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
        return new RegisterCustomerCommand("Full Name", "email@example.com", "+123456789", "Pa$$word123");
    }
}
