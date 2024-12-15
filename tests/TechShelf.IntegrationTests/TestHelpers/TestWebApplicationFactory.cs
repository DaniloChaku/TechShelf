using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Identity;
using TechShelf.IntegrationTests.TestHelpers.Seed;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TechShelf.IntegrationTests.TestHelpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private string? _connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

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
                options.SuperAdmins = [AdminHelper.SuperAdmin];
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
            using var dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_connectionString, options => options.SetPostgresVersion(12, 0))
                .Options);

            dbContext.Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }
}
