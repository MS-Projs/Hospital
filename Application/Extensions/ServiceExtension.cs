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
            .AddScoped<IUserService, UserService>()
            .AddScoped<IPatient, PatientService>()
            .AddScoped<IDoctor, DoctorService>()
            .AddScoped<ICertificateType, CertificateTypeService>()
            .AddScoped<IAppointment, AppointmentService>()
            .AddScoped<IReport, ReportService>();
        return services;
    }

    public static void ConfigureMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly()); // Scan for mappers in this assembly
        
        services.AddSingleton(config);
    }
}