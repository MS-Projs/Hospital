using System.Reflection;
using System.Text;
using Application.Extensions;
using DataAccess.Extensions;
using DataAccess.Models;
using Domain.Extensions;
using Domain.Models.Options;
using Infrastructure.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Administration.Extensions;

public static class ServiceExtension
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        //services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));
        services.AddDomain(configuration)
            .AddInfrastructure(configuration)
            .AddApplication(configuration)
            .AddDataAccess(configuration);
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
        });


        return services;
    }

    public static IServiceCollection AddSwaggerGenWithJwtTokenHeaders(this IServiceCollection services)
    {
        return services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            // Add JWT Security Definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your_token}"
            });

            // Apply JWT Security Requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });
    }

    public static void RegisterMapsterConfiguration()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(Assembly.GetExecutingAssembly());

        // Enable implicit mapping for inherited classes
        config.Default.PreserveReference(true);
        config.Default.IgnoreNullValues(true);
        config.Default.ShallowCopyForSameType(true);
    }
}