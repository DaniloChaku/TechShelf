using FluentValidation;

namespace TechShelf.Application.Features.Orders.Commands.SetPaymentStatus;

public class SetPaymentStatusCommandValidator : AbstractValidator<SetPaymentStatusCommand>
{
    public SetPaymentStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.PaymentIntentId)
            .NotEmpty()
            .DependentRules(() =>
            {
                RuleFor(x => x.PaymentIntentId)
                    .Must(id => id.StartsWith("pi_"))
                    .WithMessage("Payment Intent ID must start with 'pi_'.");
            });
    }
}
