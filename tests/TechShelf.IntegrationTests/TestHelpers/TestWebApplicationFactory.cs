using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechShelf.Domain.Common;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;
using TechShelf.IntegrationTests.TestHelpers.TestData;
using Testcontainers.PostgreSql;

namespace TechShelf.IntegrationTests.TestHelpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;

    public TestWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _serviceProvider = BuildServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
    }

    public string ConnectionString => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(ConfigureTestAppConfiguration);
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    private void ConfigureTestAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
    {
        var jwtConfiguration = new Dictionary<string, string?>
        {
            [$"Jwt:{nameof(JwtOptions.SecretKey)}"] = JwtTestHelper.Key,
            [$"Jwt:{nameof(JwtOptions.Issuer)}"] = JwtTestHelper.Issuer,
            [$"Jwt:{nameof(JwtOptions.Audience)}"] = JwtTestHelper.Audience,
            [$"Jwt:{nameof(JwtOptions.ExpiresInMinutes)}"] = JwtTestHelper.ExpiresInMinutes.ToString(),
            [$"Jwt:{nameof(JwtOptions.RefreshExpiresInDays)}"] = JwtTestHelper.RefreshExpiresInDays.ToString(),
        };

        configBuilder.AddInMemoryCollection(jwtConfiguration);
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        ConfigureDatabases(services);
        ConfigureApplicationServices(services);
        SeedDatabases(services);
    }

    private void ConfigureDatabases(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
        services.RemoveAll<DbContextOptions<AppIdentityDbContext>>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(ConnectionString));
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(ConnectionString));
    }

    private void ConfigureApplicationServices(IServiceCollection services)
    {
        services.Configure<AdminOptions>(options =>
        {
            options.SuperAdmins = [AdminHelper.SuperAdminOptions];
        });
    }

    private void SeedDatabases(IServiceCollection services)
    {
        // This order is important so that Users have ids by the time orders are seeded
        SeedIdentityDatabase(services);
        SeedApplicationDatabase(services);
    }

    private static void SeedIdentityDatabase(IServiceCollection services)
    {
        using var dbContext = CreateDbContext<AppIdentityDbContext>(services);
        dbContext.Database.Migrate();

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        SeedRoles(roleManager).Wait();
        CustomerHelper.Seed(userManager);
    }

    private static void SeedApplicationDatabase(IServiceCollection services)
    {
        using var dbContext = CreateDbContext<ApplicationDbContext>(services);
        dbContext.Database.Migrate();

        BrandHelper.Seed(dbContext);
        CategoryHelper.Seed(dbContext);
        ProductHelper.Seed(dbContext);
        OrderHelper.Seed(dbContext);
    }

    private static TDbContext CreateDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in UserRoles.GetAllRoles())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureDatabases(services);
        return services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        _serviceScope.Dispose();

        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }
}
