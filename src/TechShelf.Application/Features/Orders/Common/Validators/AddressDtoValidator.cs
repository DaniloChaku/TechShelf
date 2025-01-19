using FluentValidation;
using TechShelf.Application.Features.Orders.Common.Dtos;
using TechShelf.Domain.Entities.OrderAggregate;

namespace TechShelf.Application.Features.Orders.Common.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Country).NotEmpty()
            .MaximumLength(2)
            .Must(c => Address.AllowedCountries.Contains(c))
            .WithMessage("Country is not supported");
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
