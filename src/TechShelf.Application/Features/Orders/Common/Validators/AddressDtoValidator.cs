using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Dtos;

namespace TechShelf.Application.Features.Orders.Common.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Line1).NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.City).NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.State).NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.PostalCode).NotEmpty()
            .MaximumLength(10);
    }
}
