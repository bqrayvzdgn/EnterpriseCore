using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Application.Features.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    Guid OwnerId,
    string OwnerName,
    int MemberCount,
    int TaskCount,
    int CompletedTaskCount,
    DateTime CreatedAt);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    Guid OwnerId,
    string OwnerName,
    IEnumerable<ProjectMemberDto> Members,
    IEnumerable<MilestoneDto> Milestones,
    ProjectStatsDto Stats,
    DateTime CreatedAt);

public record ProjectMemberDto(
    Guid UserId,
    string Email,
    string FullName,
    ProjectMemberRole Role,
    DateTime JoinedAt);

public record MilestoneDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime? DueDate,
    DateTime? CompletedDate,
    int TaskCount,
    int CompletedTaskCount);

public record ProjectStatsDto(
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int OverdueTasks,
    decimal CompletionPercentage);

public record CreateProjectRequest(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget);

public record UpdateProjectRequest(
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget);

public record AddProjectMemberRequest(
    Guid UserId,
    ProjectMemberRole Role);
