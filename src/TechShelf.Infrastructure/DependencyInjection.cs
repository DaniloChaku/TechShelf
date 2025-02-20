using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using System.Text;
using TechShelf.Application.Interfaces.Auth;
using TechShelf.Application.Interfaces.Data;
using TechShelf.Application.Interfaces.Services;
using TechShelf.Infrastructure.Data;
using TechShelf.Infrastructure.Data.Interceptors;
using TechShelf.Infrastructure.Data.Outbox;
using TechShelf.Infrastructure.Data.Repositories;
using TechShelf.Infrastructure.Identity;
using TechShelf.Infrastructure.Identity.Options;
using TechShelf.Infrastructure.Identity.Services;
using TechShelf.Infrastructure.Options;
using TechShelf.Infrastructure.Services.SendGrid;
using TechShelf.Infrastructure.Services.Stripe;

namespace TechShelf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration)
            .AddOutboxServices(configuration)
            .AddRepositoryServices()
            .AddAuthenticationServices(configuration)
            .AddUserServices()
            .AddPaymentServices(configuration)
            .AddEmailServices(configuration)
            .AddTimeServices()
            .AddIdentityServices(configuration);

        return services;
    }

    private static IServiceCollection AddDatabaseServices(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Default"))
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        return services;
    }

    private static IServiceCollection AddOutboxServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DomainEventsToOutboxInterceptor>();
        services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    private static IServiceCollection AddRepositoryServices(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

        return services;
    }

    private static IServiceCollection AddUserServices(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    private static IServiceCollection AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.AddScoped<IStripeService, StripeService>();

        return services;
    }

    private static IServiceCollection AddEmailServices(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddSendGrid(options =>
        {
            options.ApiKey = configuration["SendGrid:ApiKey"];
        });
        services.AddTransient<IEmailService, SendGridService>();
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));

        return services;
    }

    private static IServiceCollection AddTimeServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static IServiceCollection AddIdentityServices(
       this IServiceCollection services,
       IConfiguration configuration)
    {
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

        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SectionName));
        services.AddScoped<IdentitySeeder>();

        IdentityMapsterConfig.Configure();

        return services;
    }
}
