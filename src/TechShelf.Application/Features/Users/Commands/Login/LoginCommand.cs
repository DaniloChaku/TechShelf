using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Features.Users.Commands.Login;

public record LoginCommand(string Email, string Password)
    : IRequest<ErrorOr<TokenDto>>;
