using System.Reflection;
using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Application.Validators.Auth;
using Domain.Models.API.Requests;
using FluentValidation;
using Mapster;
using MediatR;
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
            .AddScoped<IDocumentCategory, DocumentCategoryService>()
            .AddScoped<IReport, ReportService>();
        
        services.AddMapsterConfig();
        return services;
    }

    private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(Assembly.GetExecutingAssembly());
        config.Default.IgnoreNonMapped(true);
        return services;
    }
    
    public static void ConfigureFluentValidator()
    {
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
    }
    public static void ConfigureMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly()); // Scan for mappers in this assembly
        
        services.AddSingleton(config);
    }
}