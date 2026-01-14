namespace EnterpriseCore.Application.Features.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveSprints { get; set; }
    public IReadOnlyList<TaskStatusCountDto> TasksByStatus { get; set; } = Array.Empty<TaskStatusCountDto>();
    public IReadOnlyList<RecentActivityDto> RecentActivities { get; set; } = Array.Empty<RecentActivityDto>();
}

public class TaskStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectReportDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TodoTasks { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int TotalMembers { get; set; }
    public decimal? Budget { get; set; }
    public decimal? TotalEstimatedHours { get; set; }
    public decimal? TotalActualHours { get; set; }
    public IReadOnlyList<SprintSummaryDto> Sprints { get; set; } = Array.Empty<SprintSummaryDto>();
    public IReadOnlyList<MemberWorkloadDto> MemberWorkloads { get; set; } = Array.Empty<MemberWorkloadDto>();
}

public class SprintSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class MemberWorkloadDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int AssignedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
}

public class MyStatsDto
{
    public int TotalAssignedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int ProjectsCount { get; set; }
    public decimal? TotalLoggedHours { get; set; }
    public IReadOnlyList<TaskStatusCountDto> TasksByPriority { get; set; } = Array.Empty<TaskStatusCountDto>();
    public IReadOnlyList<UpcomingTaskDto> UpcomingTasks { get; set; } = Array.Empty<UpcomingTaskDto>();
}

public class UpcomingTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class TeamWorkloadDto
{
    public IReadOnlyList<MemberWorkloadDto> Members { get; set; } = Array.Empty<MemberWorkloadDto>();
    public int TotalTasks { get; set; }
    public int UnassignedTasks { get; set; }
}
