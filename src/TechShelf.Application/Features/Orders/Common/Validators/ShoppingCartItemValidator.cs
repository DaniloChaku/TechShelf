using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Common.Validators;

public class ShoppingCartItemValidator : AbstractValidator<ShoppingCartItem>
{
    public ShoppingCartItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100 items");
    }
}
