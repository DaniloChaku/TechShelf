using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Validators;
using TechShelf.Domain.Common;

namespace TechShelf.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(CommonConstants.PhoneNumberRegex) 
            .WithMessage("Phone number must be in a valid format (e.g. +1234567890)");


        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .SetValidator(new AddressDtoValidator());

        RuleFor(x => x.BasketItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item")
            .Must(items => items.Any())
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.BasketItems)
            .NotEmpty()
            .SetValidator(new BasketItemValidator());
    }
}