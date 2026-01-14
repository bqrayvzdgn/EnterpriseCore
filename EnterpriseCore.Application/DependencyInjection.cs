using EnterpriseCore.Application.Features.ActivityLogs.Services;
using EnterpriseCore.Application.Features.Attachments.Services;
using EnterpriseCore.Application.Features.Auth.Services;
using EnterpriseCore.Application.Features.Dashboard.Services;
using EnterpriseCore.Application.Features.Projects.Services;
using EnterpriseCore.Application.Features.Roles.Services;
using EnterpriseCore.Application.Features.Sprints.Services;
using EnterpriseCore.Application.Features.Tasks.Services;
using EnterpriseCore.Application.Features.Users.Services;
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
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<ISprintService, SprintService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
