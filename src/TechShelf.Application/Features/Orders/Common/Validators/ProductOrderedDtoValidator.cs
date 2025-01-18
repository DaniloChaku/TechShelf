using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Common.Validators;

public class ProductOrderedDtoValidator : AbstractValidator<ProductOrderedDto>
{
    public ProductOrderedDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(250);  
    }
}