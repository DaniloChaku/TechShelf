using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Validators;
using TechShelf.Domain.Common;
using TechShelf.Domain.Orders;

namespace TechShelf.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(OrderConstants.EmailMaxLength)
            .WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(CommonConstants.PhoneNumberRegex)
            .WithMessage("Phone number must be in a valid format (e.g. +1234567890).");


        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(OrderConstants.FullNameMaxLength)
            .WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new AddressDtoValidator());

        RuleFor(x => x.ShoppingCartItems)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.ShoppingCartItems)
            .NotEmpty().WithMessage("Item quantity is required.")
            .SetValidator(new ShoppingCartItemValidator());

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Only registered customers can make orders.");
    }
}