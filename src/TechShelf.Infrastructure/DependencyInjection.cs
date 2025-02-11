using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Interceptors;
using TechShelf.Infrastructure.Data.Repositories;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;
using TechShelf.Infrastructure.Identity.Services;
using TechShelf.Infrastructure.Options;
using TechShelf.Infrastructure.Services.Stripe;

namespace TechShelf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Default"))
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.AddDbContext<AppIdentityDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>();

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<DomainEventsToOutboxInterceptor>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
                };
            });
        services.AddAuthorization();

        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SectionName));
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<IUserService ,UserService>();

        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IStripeService, StripeService>();

        IdentityMapsterConfig.Configure();

        return services;
    }
}
