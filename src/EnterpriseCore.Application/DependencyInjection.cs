using EnterpriseCore.Application.Features.Auth.Services;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Application.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
