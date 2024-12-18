using Mapster;
using TechShelf.Application.Features.Users.Common;

namespace TechShelf.Infrastructure.Identity;

public static class IdentityMapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<UserDto, ApplicationUser>
            .NewConfig()
            .Map(dest => dest.UserName, src => src.Email);
    }
}
