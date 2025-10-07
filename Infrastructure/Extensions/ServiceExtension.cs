using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        HttpContextExtension.Configure(services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>());
        
        services.AddSingleton<ITokenService, TokenService>();
        return services;
    }
}