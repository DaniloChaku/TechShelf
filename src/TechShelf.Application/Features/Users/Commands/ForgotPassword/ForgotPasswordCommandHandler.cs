using System.Web;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;
using TechShelf.Application.Common.Options;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Application.Interfaces.Services;

namespace TechShelf.Application.Features.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ErrorOr<Unit>>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ClientUrlOptions _clientUrlOptions;

    public ForgotPasswordCommandHandler(
        IUserService userService,
        IEmailService emailService,
        IOptions<ClientUrlOptions> clientUrl)
    {
        _userService = userService;
        _emailService = emailService;
        _clientUrlOptions = clientUrl.Value;
    }

    public async Task<ErrorOr<Unit>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = await _userService.GetPasswordResetToken(request.Email);
        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        string token = tokenResult.Value;
        string resetUrl =
            $"{_clientUrlOptions.ClientUrl}/reset-password?token={HttpUtility.UrlEncode(token)}&email={HttpUtility.UrlEncode(request.Email)}";

        string subject = "Reset Your Password";
        string message = $"Please reset your password by clicking on the following link: {resetUrl}\n\n" +
                        "If you did not request a password reset, please ignore this email.";

        await _emailService.SendPlainTextEmailAsync(
               request.Email,
               subject,
               message,
               cancellationToken);

        return Unit.Value;
    }
}
