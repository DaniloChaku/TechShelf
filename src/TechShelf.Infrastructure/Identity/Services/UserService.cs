using ErrorOr;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TechShelf.Application.Features.Users.Common;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Identity.Errors;

namespace TechShelf.Infrastructure.Identity.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppIdentityDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        AppIdentityDbContext dbContext,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> RegisterAsync(UserDto userDto, string password, string role)
    {
        if (await _userManager.FindByEmailAsync(userDto.Email) != null)
        {
            _logger.LogWarning("Falied to add user {Email} because the email is already in use", userDto.Email);
            return UserErrors.RegistrationFalied;
        }

        var user = userDto.Adapt<ApplicationUser>();

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var registrationResult = await _userManager.CreateAsync(user, password);
            if (!registrationResult.Succeeded)
            {
                _logger.LogWarning(
                    "Falied to add user {Email}. Errors: {Errors}", 
                    userDto.Email, 
                    registrationResult.Errors.Select(e => e.Description));
                await transaction.RollbackAsync();
                return UserErrors.RegistrationFalied;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Falied to assign Cusomer role to user {Email}", userDto.Email);
                await transaction.RollbackAsync();
                return UserErrors.RegistrationFalied;
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

        return true;
    }
}
