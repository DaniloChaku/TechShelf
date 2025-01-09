using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Features.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string Email, string RefreshToken) : IRequest<ErrorOr<TokenDto>>;