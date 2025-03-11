using FluentValidation;

namespace TechShelf.Application.Features.Users.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.Password)
           .NotEmpty().WithMessage("Password is required.")
           .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
           .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
           .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
           .Matches(@"\d").WithMessage("Password must contain at least one digit.")
           .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");
    }
}
