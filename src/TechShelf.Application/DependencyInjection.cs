using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TechShelf.Application.Common.Behaviors;
using TechShelf.Application.Common.Mappings;

namespace TechShelf.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(conf =>
        {
            conf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            conf.AddOpenBehavior(typeof(LoggingBehavior<,>));
            conf.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        OrderMappings.Configure();

        return services;
    }
}
