using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechShelf.Domain.Common;

namespace TechShelf.Infrastructure.Identity;

public class IdentitySeeder
{
    private readonly AppIdentityDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminOptions _superAdminOptions;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        AppIdentityDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminOptions> options,
        ILogger<IdentitySeeder> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _superAdminOptions = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("[AppIdentityDbContext] Starting database seeding process");

            await MigrateDatabaseAsync();
            await SeedRolesAsync();
            await SeedSuperAdmins();

            _logger.LogInformation("[AppIdentityDbContext] Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppIdentityDbContext] An error occurred while seeding the db.");
        }
    }

    private async Task MigrateDatabaseAsync()
    {
        await _context.Database.MigrateAsync();
        _logger.LogInformation("[AppIdentityDbContext] Database migrations applied successfully");
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in UserRoles.GetAllRoles())
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogInformation("Creating role: {Role}", role);
                var result = await _roleManager.CreateAsync(new IdentityRole(role));

                if (result.Succeeded)
                {
                    _logger.LogInformation("Role created: {Role}", role);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to create role: {Role}. Errors: {Errors}",
                        role,
                        result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                _logger.LogInformation("Role already exists: {Role}", role);
            }
        }
    }

    private async Task SeedSuperAdmins()
    {
        foreach (var admin in _superAdminOptions.SuperAdmins)
        {
            var existingUser = await _userManager.FindByEmailAsync(admin.Email);

            if (existingUser == null)
            {
                _logger.LogInformation("Creating super admin user: {Email}", admin.Email);

                var user = new ApplicationUser
                {
                    UserName = admin.Email,
                    Email = admin.Email,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, admin.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Super admin user created: {Email}", admin.Email);
                    await _userManager.AddToRoleAsync(user, "SuperAdmin");
                    _logger.LogInformation("Added super admin role to user: {Email}", admin.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create super admin user: {Email}. Errors: {Errors}",
                                       admin.Email,
                                       result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                _logger.LogInformation("Super admin user already exists: {Email}", admin.Email);
            }
        }
    }
}
