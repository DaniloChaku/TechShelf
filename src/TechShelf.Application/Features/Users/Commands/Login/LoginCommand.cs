using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Users.Commands.Login;

public record LoginCommand(string Email, string Password)
    : IRequest<ErrorOr<string>>;
