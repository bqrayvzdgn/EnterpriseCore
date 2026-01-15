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
        // Server-side aggregation for projects
        var projectStats = await _projectRepository.Query()
            .GroupBy(p => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(p => p.Status == ProjectStatus.Active)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Server-side aggregation for tasks
        var taskStats = await _taskRepository.Query()
            .GroupBy(t => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(t => t.Status == TaskItemStatus.Done),
                Overdue = g.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Tasks by status - server-side grouping
        var tasksByStatus = await _taskRepository.Query()
            .GroupBy(t => t.Status)
            .Select(g => new TaskStatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var users = await _userRepository.Query().CountAsync(u => u.IsActive, cancellationToken);
        var activeSprints = await _sprintRepository.Query()
            .CountAsync(s => s.Status == SprintStatus.Active, cancellationToken);

        // Recent activities with projection
        var recentActivities = await _dbContext.Set<ActivityLog>()
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto
        {
            TotalProjects = projectStats?.Total ?? 0,
            ActiveProjects = projectStats?.Active ?? 0,
            TotalTasks = taskStats?.Total ?? 0,
            CompletedTasks = taskStats?.Completed ?? 0,
            OverdueTasks = taskStats?.Overdue ?? 0,
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
            .AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure<ProjectReportDto>("Project not found", "NOT_FOUND");
        }

        // Get task statistics in a single query
        var taskStats = await _taskRepository.Query()
            .Where(t => t.ProjectId == projectId)
            .GroupBy(t => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(t => t.Status == TaskItemStatus.Done),
                InProgress = g.Count(t => t.Status == TaskItemStatus.InProgress),
                Todo = g.Count(t => t.Status == TaskItemStatus.Todo),
                TotalEstimatedHours = g.Sum(t => t.EstimatedHours ?? 0),
                TotalActualHours = g.Sum(t => t.ActualHours ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Get member workloads with server-side aggregation
        var memberWorkloads = await _dbContext.Set<ProjectMember>()
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId)
            .Select(pm => new
            {
                pm.UserId,
                UserName = pm.User.FirstName + " " + pm.User.LastName
            })
            .Join(
                _taskRepository.Query()
                    .Where(t => t.ProjectId == projectId)
                    .GroupBy(t => t.AssigneeId)
                    .Select(g => new
                    {
                        AssigneeId = g.Key,
                        AssignedTasks = g.Count(),
                        CompletedTasks = g.Count(t => t.Status == TaskItemStatus.Done),
                        InProgressTasks = g.Count(t => t.Status == TaskItemStatus.InProgress)
                    }),
                pm => pm.UserId,
                t => t.AssigneeId,
                (pm, t) => new MemberWorkloadDto
                {
                    UserId = pm.UserId,
                    UserName = pm.UserName,
                    AssignedTasks = t.AssignedTasks,
                    CompletedTasks = t.CompletedTasks,
                    InProgressTasks = t.InProgressTasks
                })
            .ToListAsync(cancellationToken);

        // Get members without tasks
        var membersWithoutTasks = await _dbContext.Set<ProjectMember>()
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId && !memberWorkloads.Select(m => m.UserId).Contains(pm.UserId))
            .Select(pm => new MemberWorkloadDto
            {
                UserId = pm.UserId,
                UserName = pm.User.FirstName + " " + pm.User.LastName,
                AssignedTasks = 0,
                CompletedTasks = 0,
                InProgressTasks = 0
            })
            .ToListAsync(cancellationToken);

        memberWorkloads.AddRange(membersWithoutTasks);

        // Get sprint summaries
        var sprintSummaries = await _sprintRepository.Query()
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SprintSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Status = s.Status.ToString(),
                TotalTasks = s.Tasks != null ? s.Tasks.Count : 0,
                CompletedTasks = s.Tasks != null ? s.Tasks.Count(t => t.Status == TaskItemStatus.Done) : 0,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
            .ToListAsync(cancellationToken);

        var totalTasks = taskStats?.Total ?? 0;
        var completedTasks = taskStats?.Completed ?? 0;

        var report = new ProjectReportDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = project.Status.ToString(),
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = taskStats?.InProgress ?? 0,
            TodoTasks = taskStats?.Todo ?? 0,
            CompletionPercentage = totalTasks > 0 ? Math.Round((decimal)completedTasks / totalTasks * 100, 2) : 0,
            TotalMembers = memberWorkloads.Count,
            Budget = project.Budget,
            TotalEstimatedHours = taskStats?.TotalEstimatedHours,
            TotalActualHours = taskStats?.TotalActualHours,
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

        // Get task statistics in a single query
        var taskStats = await _taskRepository.Query()
            .Where(t => t.AssigneeId == userId.Value)
            .GroupBy(t => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(t => t.Status == TaskItemStatus.Done),
                InProgress = g.Count(t => t.Status == TaskItemStatus.InProgress),
                Overdue = g.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done),
                TotalLoggedHours = g.Sum(t => t.ActualHours ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var myProjects = await _dbContext.Set<ProjectMember>()
            .Where(pm => pm.UserId == userId.Value)
            .Select(pm => pm.ProjectId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Tasks by priority - server-side grouping
        var tasksByPriority = await _taskRepository.Query()
            .Where(t => t.AssigneeId == userId.Value)
            .GroupBy(t => t.Priority)
            .Select(g => new TaskStatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        // Upcoming tasks with projection
        var upcomingTasks = await _taskRepository.Query()
            .AsNoTracking()
            .Where(t => t.AssigneeId == userId.Value && t.DueDate.HasValue && t.DueDate > DateTime.UtcNow && t.Status != TaskItemStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .Select(t => new UpcomingTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project != null ? t.Project.Name : string.Empty,
                DueDate = t.DueDate,
                Priority = t.Priority.ToString()
            })
            .ToListAsync(cancellationToken);

        var stats = new MyStatsDto
        {
            TotalAssignedTasks = taskStats?.Total ?? 0,
            CompletedTasks = taskStats?.Completed ?? 0,
            InProgressTasks = taskStats?.InProgress ?? 0,
            OverdueTasks = taskStats?.Overdue ?? 0,
            ProjectsCount = myProjects,
            TotalLoggedHours = taskStats?.TotalLoggedHours,
            TasksByPriority = tasksByPriority,
            UpcomingTasks = upcomingTasks
        };

        return Result.Success(stats);
    }

    public async Task<Result<TeamWorkloadDto>> GetTeamWorkloadAsync(CancellationToken cancellationToken = default)
    {
        // Get task counts per assignee in a single query
        var tasksByAssignee = await _taskRepository.Query()
            .Where(t => t.AssigneeId != null)
            .GroupBy(t => t.AssigneeId)
            .Select(g => new
            {
                AssigneeId = g.Key,
                AssignedTasks = g.Count(),
                CompletedTasks = g.Count(t => t.Status == TaskItemStatus.Done),
                InProgressTasks = g.Count(t => t.Status == TaskItemStatus.InProgress)
            })
            .ToListAsync(cancellationToken);

        var assigneeIds = tasksByAssignee.Select(t => t.AssigneeId).ToList();

        // Get user names for assignees
        var userNames = await _userRepository.Query()
            .AsNoTracking()
            .Where(u => assigneeIds.Contains(u.Id))
            .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var memberWorkloads = tasksByAssignee
            .Where(t => t.AssigneeId.HasValue)
            .Select(t => new MemberWorkloadDto
            {
                UserId = t.AssigneeId!.Value,
                UserName = userNames.GetValueOrDefault(t.AssigneeId.Value, "Unknown"),
                AssignedTasks = t.AssignedTasks,
                CompletedTasks = t.CompletedTasks,
                InProgressTasks = t.InProgressTasks
            })
            .OrderByDescending(m => m.AssignedTasks)
            .ToList();

        // Get total and unassigned task counts
        var totalTasks = await _taskRepository.Query().CountAsync(cancellationToken);
        var unassignedTasks = await _taskRepository.Query().CountAsync(t => t.AssigneeId == null, cancellationToken);

        var workload = new TeamWorkloadDto
        {
            Members = memberWorkloads,
            TotalTasks = totalTasks,
            UnassignedTasks = unassignedTasks
        };

        return Result.Success(workload);
    }
}
