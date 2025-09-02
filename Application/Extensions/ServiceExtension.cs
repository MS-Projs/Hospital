using System.Reflection;
using Application.Interfaces;
using Application.Services;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IUserService,
                UserService>();
        return services;
    }

    public static void ConfigureValidator(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly()); // Ensure all mappers are loaded

        services.AddSingleton(config);
    }
}