using ErrorOr;
using MediatR;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Application.Features.Users.Queries.GetUserInfo;

public record GetUserInfoQuery(string Email) : IRequest<ErrorOr<UserDto>>;
