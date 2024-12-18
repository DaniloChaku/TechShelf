using ErrorOr;
using MediatR;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Errors;

namespace TechShelf.Application.Features.Users.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, ErrorOr<string>>
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
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

        return tokenResult.Value;
    }
}
