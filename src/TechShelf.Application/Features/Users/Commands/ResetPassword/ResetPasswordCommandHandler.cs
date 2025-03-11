using ErrorOr;
using MediatR;
using TechShelf.Application.Interfaces.Auth;

namespace TechShelf.Application.Features.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ErrorOr<Unit>>
{
    private readonly IUserService _userService;

    public ResetPasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ErrorOr<Unit>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.ResetPassword(
            email: request.Email, token: request.Token, newPassword: request.Password);

        if (result.IsError)
        {
            return result.Errors;
        }

        return Unit.Value;
    }
}
