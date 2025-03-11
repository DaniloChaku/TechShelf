using FluentValidation;

namespace TechShelf.Application.Features.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotNull().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
