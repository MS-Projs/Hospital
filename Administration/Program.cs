using Administration.Extensions;
using NLog;
using NLog.Web;
using Microsoft.OpenApi.Models;

var logger = LogManager
    .Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    ServiceExtension.RegisterMapsterConfiguration();
    builder.Services
        .AddAuthentication(builder.Configuration)
        .RegisterServices(builder.Configuration);

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    
    // Swagger konfiguratsiyasi JWT autentifikatsiya bilan
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Administration API",
            Version = "v1",
            Description = "API documentation for Administration service"
        });

        // JWT Bearer token qo'shish
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below."
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

        // XML hujjatlarni qo'shish (ixtiyoriy)
        // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        // options.IncludeXmlComments(xmlPath);
    });

    // Enable serving static files from wwwroot folder
    builder.Services.AddDirectoryBrowser();

    // Configure static file options
    builder.Services.Configure<StaticFileOptions>(options =>
    {
        options.ServeUnknownFileTypes = true; // Serve all file types
    });

   
    var app = builder.Build();

    // Swagger barcha muhitlarda ishlashi uchun
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Administration API v1");
        options.RoutePrefix = "swagger"; // https://localhost:port/swagger
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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