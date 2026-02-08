using Administration.Extensions;
using Application.Validators;
using Application.Validators.Auth;
using DataAccess;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;

var logger = LogManager
    .Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5255); 
    });
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    ServiceExtension.RegisterMapsterConfiguration();

    builder.Services
        .AddAuthentication(builder.Configuration)
        .RegisterServices(builder.Configuration);

    // 🚀 FluentValidation avtomatik integratsiyasi
    builder.Services
        .AddControllers()
        .AddFluentValidation(fv =>
        {
            // Barcha validatorlarni avtomatik yuklab oladi
            fv.RegisterValidatorsFromAssembly(typeof(SignUpRequestValidator).Assembly);
            fv.ImplicitlyValidateChildProperties = true;
            fv.ImplicitlyValidateRootCollectionElements = true;
        });

    builder.Services.AddEndpointsApiExplorer();

    // Swagger konfiguratsiyasi
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Administration API",
            Version = "v1",
            Description = "API documentation for Administration service"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();
    
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Administration API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    MigrateDatabase(app);
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application encountered a critical error.");
    throw;
}
finally
{
    LogManager.Shutdown();
}

void MigrateDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<EntityContext>();

        // ✅ Faqat Migrate() ishlatilsin
        context.Database.Migrate();

        logger.LogCritical("Database migration completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during migration");
        throw; // Re-throw to stop application startup
    }
}