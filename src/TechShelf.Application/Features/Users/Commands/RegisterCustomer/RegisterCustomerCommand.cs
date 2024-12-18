using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Users.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password)
    : IRequest<ErrorOr<string>>;
