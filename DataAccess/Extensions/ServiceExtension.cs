using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataAccess.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)

    {
        services.AddDbContext<EntityContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;

    }
}