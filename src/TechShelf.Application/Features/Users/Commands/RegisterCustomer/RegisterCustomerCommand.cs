using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Features.Users.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password)
    : IRequest<ErrorOr<TokenDto>>;
