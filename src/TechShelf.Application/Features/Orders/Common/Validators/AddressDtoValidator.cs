﻿using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Orders;

namespace TechShelf.Application.Features.Orders.Common.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Line1)
            .NotEmpty().WithMessage("Address Line 1 is required.")
            .MaximumLength(OrderConstants.Address.Line1MaxLength)
            .WithMessage($"Address Line 1 must not exceed {OrderConstants.Address.Line1MaxLength} characters.");
        RuleFor(x => x.Line2)
            .Must(x => x is null || x.Length <= OrderConstants.Address.Line2MaxLength)
            .WithMessage($"Address Line 2 must not exceed {OrderConstants.Address.Line2MaxLength} characters");
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(OrderConstants.Address.CityMaxLength)
            .WithMessage($"City must not exceed {OrderConstants.Address.Line2MaxLength} characters.");
        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(OrderConstants.Address.StateMaxLength)
            .WithMessage($"State must not exceed {OrderConstants.Address.StateMaxLength} characters.");
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal Code is required.")
            .MaximumLength(OrderConstants.Address.PostalCodeMaxLength)
            .WithMessage($"Postal Code must not exceed {OrderConstants.Address.PostalCodeMaxLength} characters.");
    }
}
