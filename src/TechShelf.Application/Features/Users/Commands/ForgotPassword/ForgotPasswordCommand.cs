using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ErrorOr<Unit>>;
