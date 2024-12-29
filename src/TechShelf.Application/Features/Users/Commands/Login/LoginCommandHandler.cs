using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Errors;

namespace TechShelf.Application.Features.Users.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, ErrorOr<TokenDto>>
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<TokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var passwordValidationResult = await _userService.ValidatePasswordAsync(request.Email, request.Password);
        if (passwordValidationResult.IsError) return passwordValidationResult.Errors;

        bool isPasswordValid = passwordValidationResult.Value;
        if (!isPasswordValid)
        {
            return UserErrors.LoginAttemptFailed;
        }

        var tokenResult = await _tokenService.GetTokenAsync(request.Email);
        if (tokenResult.IsError) return tokenResult.Errors;

        var refreshTokenResult = await _tokenService.GetRefreshTokenAsync(request.Email);
        if (refreshTokenResult.IsError) return refreshTokenResult.Errors;

        return new TokenDto(tokenResult.Value, refreshTokenResult.Value);
    }
}
