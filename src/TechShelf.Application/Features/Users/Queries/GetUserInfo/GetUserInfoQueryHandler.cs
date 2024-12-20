using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;

namespace TechShelf.Application.Features.Users.Queries.GetUserInfo;

public class GetUserInfoQueryHandler
    : IRequestHandler<GetUserInfoQuery, ErrorOr<UserDto>>
{
    private readonly IUserService _userService;

    public GetUserInfoQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var userResult = await _userService.GetUserByEmailAsync(request.Email);

        return userResult;
    }
}
