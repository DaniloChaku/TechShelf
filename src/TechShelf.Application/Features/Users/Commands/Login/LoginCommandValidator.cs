using FluentValidation;
using TechShelf.Domain.Users;

namespace TechShelf.Application.Features.Users.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(UserConstants.EmailMaxLength)
            .WithMessage($"Email must not exceed {UserConstants.EmailMaxLength} characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(UserConstants.PasswordMinLength)
            .WithMessage($"Password must be at least {UserConstants.PasswordMinLength} characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");
    }
}
