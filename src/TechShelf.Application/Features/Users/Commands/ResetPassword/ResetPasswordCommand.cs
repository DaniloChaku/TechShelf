using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string Email, string Password)
    : IRequest<ErrorOr<Unit>>;