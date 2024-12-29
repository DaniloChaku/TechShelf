using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Errors;

namespace TechShelf.Application.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ErrorOr<TokenDto>>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var isTokenValid = await _tokenService.ValidateRefreshTokenAsync(request.Email, request.RefreshToken);

        if (!isTokenValid) return UserErrors.InvalidRefreshToken;

        var tokenResult = await _tokenService.GetTokenAsync(request.Email);
        if (tokenResult.IsError) return tokenResult.Errors;

        var refreshTokenResult = await _tokenService.GetRefreshTokenAsync(request.Email);
        if (refreshTokenResult.IsError) return refreshTokenResult.Errors;

        return new TokenDto(tokenResult.Value, refreshTokenResult.Value);
    }
}
