using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;
using TechShelf.IntegrationTests.TestHelpers.Seed;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TechShelf.IntegrationTests.TestHelpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private string? _connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                [$"Jwt:{nameof(JwtOptions.SecretKey)}"] = JwtTestHelper.Key,
                [$"Jwt:{nameof(JwtOptions.Issuer)}"] = JwtTestHelper.Issuer,
                [$"Jwt:{nameof(JwtOptions.Audience)}"] = JwtTestHelper.Audience,
                [$"Jwt:{nameof(JwtOptions.ExpiresInMinutes)}"] = JwtTestHelper.ExpiresInMinutes.ToString(),
                [$"Jwt:{nameof(JwtOptions.RefreshExpiresInDays)}"] = JwtTestHelper.RefreshExpiresInDays.ToString(),
            };

            configBuilder.AddInMemoryCollection(configuration);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<AppIdentityDbContext>>();

            _connectionString = GetConnectionString();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString, options => options.SetPostgresVersion(12, 0)));

            using var applicationDbContext = CreateDbContext<ApplicationDbContext>(services);
            applicationDbContext.Database.EnsureDeleted();
            applicationDbContext.Database.Migrate();

            BrandHelper.Seed(applicationDbContext);
            CategoryHelper.Seed(applicationDbContext);
            ProductHelper.Seed(applicationDbContext);

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseNpgsql(_connectionString, options => options.SetPostgresVersion(12, 0)));
            services.Configure<AdminOptions>(options =>
            {
                options.SuperAdmins = [AdminHelper.SuperAdminOptions];
            });
        });
    }

    private static string? GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TestWebApplicationFactory>()
            .Build();

        var connectionString = configuration.GetConnectionString("TestDatabase");
        return connectionString;
    }

    private static IDbContext CreateDbContext<IDbContext>(IServiceCollection services)
        where IDbContext : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        return dbContext;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !string.IsNullOrEmpty(_connectionString))
        {
            var appDbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_connectionString, options => options.SetPostgresVersion(12, 0))
                .Options;

            using (var dbContext = new ApplicationDbContext(appDbContextOptions))
            {
                dbContext.Database.EnsureDeleted();
            }

            var identityDbContextOptions = new DbContextOptionsBuilder<AppIdentityDbContext>()
                .UseNpgsql(_connectionString, options => options.SetPostgresVersion(12, 0))
                .Options;

            using (var appIdentityDbContext = new AppIdentityDbContext(identityDbContextOptions))
            {
                appIdentityDbContext.Database.EnsureDeleted();
            }
        }

        base.Dispose(disposing);
    }
}
