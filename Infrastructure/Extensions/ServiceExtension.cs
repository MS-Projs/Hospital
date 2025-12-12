using Domain.Models.Options;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddHttpClient<IEskizClient, EskizClient>((serviceProvider, client) =>
        {
            var smsOptions = serviceProvider.GetRequiredService<IOptions<SmsOptions>>().Value;
    
            client.BaseAddress = new Uri(smsOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<ISmsService, SmsService>();

        services.AddScoped<IFileService, FileService>();
        return services;
    }
}