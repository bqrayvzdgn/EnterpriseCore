using AutoMapper;
using EnterpriseCore.Application.Features.Projects.DTOs;
using EnterpriseCore.Application.Features.Roles.DTOs;
using EnterpriseCore.Application.Features.Tasks.DTOs;
using EnterpriseCore.Application.Features.Users.DTOs;
using EnterpriseCore.Domain.Entities;

namespace EnterpriseCore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserListDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<User, UserDetailDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role)))
            .ForMember(d => d.ProjectCount, opt => opt.MapFrom(s => s.ProjectMemberships.Count))
            .ForMember(d => d.AssignedTaskCount, opt => opt.MapFrom(s => s.AssignedTasks.Count));

        // Role mappings
        CreateMap<Role, RoleDto>();
        CreateMap<Role, RoleListDto>()
            .ForMember(d => d.UserCount, opt => opt.MapFrom(s => s.UserRoles.Count))
            .ForMember(d => d.PermissionCount, opt => opt.MapFrom(s => s.RolePermissions.Count));

        CreateMap<Role, RoleDetailDto>()
            .ForMember(d => d.Permissions, opt => opt.MapFrom(s => s.RolePermissions.Select(rp => rp.Permission)));

        // Permission mappings
        CreateMap<Permission, PermissionDto>();

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => $"{s.Owner.FirstName} {s.Owner.LastName}"))
            .ForMember(d => d.MemberCount, opt => opt.MapFrom(s => s.Members.Count))
            .ForMember(d => d.TaskCount, opt => opt.MapFrom(s => s.Tasks.Count))
            .ForMember(d => d.CompletedTaskCount, opt => opt.MapFrom(s => s.Tasks.Count(t => t.Status == Domain.Enums.TaskItemStatus.Done)));

        CreateMap<Project, ProjectDetailDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => $"{s.Owner.FirstName} {s.Owner.LastName}"))
            .ForMember(d => d.Members, opt => opt.MapFrom(s => s.Members));

        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User.Email))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"));

        // Milestone mappings
        CreateMap<Milestone, MilestoneDto>()
            .ForMember(d => d.TaskCount, opt => opt.MapFrom(s => s.Tasks.Count))
            .ForMember(d => d.CompletedTaskCount, opt => opt.MapFrom(s => s.Tasks.Count(t => t.Status == Domain.Enums.TaskItemStatus.Done)));

        // Task mappings
        CreateMap<TaskItem, TaskDto>()
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
            .ForMember(d => d.AssigneeName, opt => opt.MapFrom(s => s.Assignee != null ? $"{s.Assignee.FirstName} {s.Assignee.LastName}" : null))
            .ForMember(d => d.MilestoneName, opt => opt.MapFrom(s => s.Milestone != null ? s.Milestone.Name : null))
            .ForMember(d => d.SubTaskCount, opt => opt.MapFrom(s => s.SubTasks.Count))
            .ForMember(d => d.CommentCount, opt => opt.MapFrom(s => s.Comments.Count));

        CreateMap<TaskItem, TaskDetailDto>()
            .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
            .ForMember(d => d.AssigneeName, opt => opt.MapFrom(s => s.Assignee != null ? $"{s.Assignee.FirstName} {s.Assignee.LastName}" : null))
            .ForMember(d => d.MilestoneName, opt => opt.MapFrom(s => s.Milestone != null ? s.Milestone.Name : null))
            .ForMember(d => d.SubTasks, opt => opt.MapFrom(s => s.SubTasks))
            .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.Comments));

        // Comment mappings
        CreateMap<TaskComment, TaskCommentDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"));
    }
}
