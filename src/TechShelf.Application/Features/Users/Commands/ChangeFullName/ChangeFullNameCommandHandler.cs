using ErrorOr;
using MediatR;
using TechShelf.Application.Interfaces.Auth;

namespace TechShelf.Application.Features.Users.Commands.ChangeFullName;

public class ChangeFullNameCommandHandler : IRequestHandler<ChangeFullNameCommand, ErrorOr<bool>>
{
    private readonly IUserService _userService;

    public ChangeFullNameCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ErrorOr<bool>> Handle(ChangeFullNameCommand request, CancellationToken cancellationToken)
    {
        return await _userService.ChangeFullName(request.UserId, request.FullName);
    }
}
