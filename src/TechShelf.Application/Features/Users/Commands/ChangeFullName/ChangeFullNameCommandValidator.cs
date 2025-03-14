﻿using FluentValidation;

namespace TechShelf.Application.Features.Users.Commands.ChangeFullName;

public class ChangeFullNameCommandValidator : AbstractValidator<ChangeFullNameCommand>
{
    public ChangeFullNameCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.");
    }
}
