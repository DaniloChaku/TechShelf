using Microsoft.AspNetCore.Authorization;
using TechShelf.Domain.Common;

namespace TechShelf.API.Common.Requirements;

public class AllowAnonymousAndCustomer : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (!context.User.Identity!.IsAuthenticated ||
            context.User.IsInRole(UserRoles.Customer))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }
}