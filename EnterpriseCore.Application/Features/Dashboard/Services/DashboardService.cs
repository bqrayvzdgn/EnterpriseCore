using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Dashboard.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Sprint> _sprintRepository;
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        IRepository<User> userRepository,
        IRepository<Sprint> sprintRepository,
        DbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _sprintRepository = sprintRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.Query().ToListAsync(cancellationToken);
        var tasks = await _taskRepository.Query().ToListAsync(cancellationToken);
        var users = await _userRepository.Query().Where(u => u.IsActive).CountAsync(cancellationToken);
        var activeSprints = await _sprintRepository.Query()
            .CountAsync(s => s.Status == SprintStatus.Active, cancellationToken);

        var tasksByStatus = tasks
            .GroupBy(t => t.Status)
            .Select(g => new TaskStatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToList();

        var recentActivities = await _dbContext.Set<ActivityLog>()
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto
        {
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.Status == ProjectStatus.Active),
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == TaskItemStatus.Done),
            OverdueTasks = tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done),
            TotalUsers = users,
            ActiveSprints = activeSprints,
            TasksByStatus = tasksByStatus,
            RecentActivities = recentActivities
        };

        return Result.Success(summary);
    }

    public async Task<Result<ProjectReportDto>> GetProjectReportAsync(
        Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.Query()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure<ProjectReportDto>("Project not found", "NOT_FOUND");
        }

        var tasks = await _taskRepository.Query()
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var members = await _dbContext.Set<ProjectMember>()
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var sprints = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);

        var memberWorkloads = members.Select(m => new MemberWorkloadDto
        {
            UserId = m.UserId,
            UserName = $"{m.User.FirstName} {m.User.LastName}",
            AssignedTasks = tasks.Count(t => t.AssigneeId == m.UserId),
            CompletedTasks = tasks.Count(t => t.AssigneeId == m.UserId && t.Status == TaskItemStatus.Done),
            InProgressTasks = tasks.Count(t => t.AssigneeId == m.UserId && t.Status == TaskItemStatus.InProgress)
        }).ToList();

        var sprintSummaries = sprints.Select(s => new SprintSummaryDto
        {
            Id = s.Id,
            Name = s.Name,
            Status = s.Status.ToString(),
            TotalTasks = s.Tasks?.Count ?? 0,
            CompletedTasks = s.Tasks?.Count(t => t.Status == TaskItemStatus.Done) ?? 0,
            StartDate = s.StartDate,
            EndDate = s.EndDate
        }).ToList();

        var completedTasks = tasks.Count(t => t.Status == TaskItemStatus.Done);
        var totalTasks = tasks.Count;

        var report = new ProjectReportDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = project.Status.ToString(),
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = tasks.Count(t => t.Status == TaskItemStatus.InProgress),
            TodoTasks = tasks.Count(t => t.Status == TaskItemStatus.Todo),
            CompletionPercentage = totalTasks > 0 ? Math.Round((decimal)completedTasks / totalTasks * 100, 2) : 0,
            TotalMembers = members.Count,
            Budget = project.Budget,
            TotalEstimatedHours = tasks.Where(t => t.EstimatedHours.HasValue).Sum(t => t.EstimatedHours),
            TotalActualHours = tasks.Where(t => t.ActualHours.HasValue).Sum(t => t.ActualHours),
            Sprints = sprintSummaries,
            MemberWorkloads = memberWorkloads
        };

        return Result.Success(report);
    }

    public async Task<Result<MyStatsDto>> GetMyStatsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.Failure<MyStatsDto>("User not authenticated", "UNAUTHORIZED");
        }

        var myTasks = await _taskRepository.Query()
            .Include(t => t.Project)
            .Where(t => t.AssigneeId == userId.Value)
            .ToListAsync(cancellationToken);

        var myProjects = await _dbContext.Set<ProjectMember>()
            .Where(pm => pm.UserId == userId.Value)
            .Select(pm => pm.ProjectId)
            .Distinct()
            .CountAsync(cancellationToken);

        var tasksByPriority = myTasks
            .GroupBy(t => t.Priority)
            .Select(g => new TaskStatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToList();

        var upcomingTasks = myTasks
            .Where(t => t.DueDate.HasValue && t.DueDate > DateTime.UtcNow && t.Status != TaskItemStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .Select(t => new UpcomingTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project?.Name ?? string.Empty,
                DueDate = t.DueDate,
                Priority = t.Priority.ToString()
            })
            .ToList();

        var stats = new MyStatsDto
        {
            TotalAssignedTasks = myTasks.Count,
            CompletedTasks = myTasks.Count(t => t.Status == TaskItemStatus.Done),
            InProgressTasks = myTasks.Count(t => t.Status == TaskItemStatus.InProgress),
            OverdueTasks = myTasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done),
            ProjectsCount = myProjects,
            TotalLoggedHours = myTasks.Where(t => t.ActualHours.HasValue).Sum(t => t.ActualHours),
            TasksByPriority = tasksByPriority,
            UpcomingTasks = upcomingTasks
        };

        return Result.Success(stats);
    }

    public async Task<Result<TeamWorkloadDto>> GetTeamWorkloadAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.Query()
            .Include(t => t.Assignee)
            .ToListAsync(cancellationToken);

        var users = await _userRepository.Query()
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);

        var memberWorkloads = users.Select(u => new MemberWorkloadDto
        {
            UserId = u.Id,
            UserName = $"{u.FirstName} {u.LastName}",
            AssignedTasks = tasks.Count(t => t.AssigneeId == u.Id),
            CompletedTasks = tasks.Count(t => t.AssigneeId == u.Id && t.Status == TaskItemStatus.Done),
            InProgressTasks = tasks.Count(t => t.AssigneeId == u.Id && t.Status == TaskItemStatus.InProgress)
        })
        .Where(m => m.AssignedTasks > 0)
        .OrderByDescending(m => m.AssignedTasks)
        .ToList();

        var workload = new TeamWorkloadDto
        {
            Members = memberWorkloads,
            TotalTasks = tasks.Count,
            UnassignedTasks = tasks.Count(t => t.AssigneeId == null)
        };

        return Result.Success(workload);
    }
}
