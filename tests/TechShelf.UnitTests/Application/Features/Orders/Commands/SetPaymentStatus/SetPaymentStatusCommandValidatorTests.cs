using FluentValidation.TestHelper;
using TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;

namespace TechShelf.UnitTests.Application.Features.Orders.Commands.SetPaymentStatus;

public class SetPaymentStatusCommandValidatorTests
{
    private readonly SetPaymentStatusCommandValidator _validator =
        new SetPaymentStatusCommandValidator();

    [Fact]
    public void Validator_HasError_WhenOrderIdIsEmpty()
    {
        // Arrange
        var command = new SetPaymentStatusCommand(Guid.Empty, true, "pi_123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void Validator_HasNoError_WhenOrderIdIsValid()
    {
        // Arrange
        var command = new SetPaymentStatusCommand(Guid.NewGuid(), true, "pi_123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OrderId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("paymentIntent")]
    public void Validator_HasError_WhenPaymentIntentIdIsInvalid(string? invalidPaymentIntentId)
    {
        // Arrange
        var command = new SetPaymentStatusCommand(Guid.NewGuid(), true, invalidPaymentIntentId!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentIntentId);
    }

    [Theory]
    [InlineData("pi_123")]
    [InlineData("pi_2H6fa78")]
    public void Validator_HasNoError_WhenPaymentIntentIdIsValid(string paymentIntentId)
    {
        // Arrange
        var command = new SetPaymentStatusCommand(Guid.NewGuid(), true, paymentIntentId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentIntentId);
    }
}
