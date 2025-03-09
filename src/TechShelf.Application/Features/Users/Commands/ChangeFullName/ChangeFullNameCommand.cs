using ErrorOr;
using MediatR;

namespace TechShelf.Application.Features.Users.Commands.ChangeFullName;

public record ChangeFullNameCommand(string UserId, string FullName) : IRequest<ErrorOr<bool>>;
