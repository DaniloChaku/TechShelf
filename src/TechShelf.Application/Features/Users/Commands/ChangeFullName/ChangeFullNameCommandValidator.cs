using FluentValidation;
using TechShelf.Domain.Users;

namespace TechShelf.Application.Features.Users.Commands.ChangeFullName;

public class ChangeFullNameCommandValidator : AbstractValidator<ChangeFullNameCommand>
{
    public ChangeFullNameCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(UserConstants.FullNameMaxLength)
            .WithMessage($"Full Name must not exceed {UserConstants.FullNameMaxLength} characters.");
    }
}
