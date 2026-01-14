using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Tasks.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<TaskDto>>> GetTasksByProjectAsync(
        Guid projectId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verify project exists and user has access
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure<PagedResult<TaskDto>>("Project not found.", "NOT_FOUND");
        }

        if (!HasProjectAccess(project, _currentUser.UserId))
        {
            return Result.Failure<PagedResult<TaskDto>>("You don't have access to this project.", "FORBIDDEN");
        }

        var query = _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var taskDtos = tasks.Select(MapToTaskDto).ToList();

        var result = new PagedResult<TaskDto>(taskDtos, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<TaskDetailDto>> GetTaskByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
                .ThenInclude(st => st.Assignee)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskDetailDto>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            return Result.Failure<TaskDetailDto>("You don't have access to this task.", "FORBIDDEN");
        }

        var subTaskDtos = task.SubTasks.Select(MapToTaskDto).ToList();

        var commentDtos = task.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.Content,
                c.UserId,
                $"{c.User?.FirstName} {c.User?.LastName}".Trim(),
                c.CreatedAt)).ToList();

        var dto = new TaskDetailDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.EstimatedHours,
            task.ActualHours,
            task.ProjectId,
            task.Project.Name,
            task.AssigneeId,
            task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
            task.MilestoneId,
            task.Milestone?.Name,
            task.ParentTaskId,
            subTaskDtos,
            commentDtos,
            task.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<TaskDto>> CreateTaskAsync(
        Guid projectId,
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            return Result.Failure<TaskDto>("User not authenticated.", "UNAUTHORIZED");
        }

        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure<TaskDto>("Project not found.", "NOT_FOUND");
        }

        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            return Result.Failure<TaskDto>("You don't have permission to create tasks in this project.", "FORBIDDEN");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = TaskItemStatus.Todo,
            Priority = request.Priority,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            AssigneeId = request.AssigneeId,
            MilestoneId = request.MilestoneId,
            ParentTaskId = request.ParentTaskId,
            ProjectId = projectId,
            TenantId = _currentUser.TenantId.Value
        };

        _dbContext.Set<TaskItem>().Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load navigation properties for response
        await _dbContext.Entry(task).Reference(t => t.Project).LoadAsync(cancellationToken);
        if (task.AssigneeId.HasValue)
        {
            await _dbContext.Entry(task).Reference(t => t.Assignee).LoadAsync(cancellationToken);
        }

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskDto>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            return Result.Failure<TaskDto>("You don't have permission to update this task.", "FORBIDDEN");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.EstimatedHours = request.EstimatedHours;
        task.ActualHours = request.ActualHours;
        task.AssigneeId = request.AssigneeId;
        task.MilestoneId = request.MilestoneId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result> DeleteTaskAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            return Result.Failure("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            return Result.Failure("You don't have permission to delete this task.", "FORBIDDEN");
        }

        // Soft delete
        _dbContext.Set<TaskItem>().Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TaskDto>> UpdateStatusAsync(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskDto>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            return Result.Failure<TaskDto>("You don't have permission to update this task.", "FORBIDDEN");
        }

        var oldStatus = task.Status;
        task.Status = request.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<TaskDto>> AssignTaskAsync(
        Guid id,
        AssignTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskDto>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            return Result.Failure<TaskDto>("You don't have permission to assign this task.", "FORBIDDEN");
        }

        // Verify assignee is a project member (if not null)
        if (request.AssigneeId.HasValue)
        {
            var isMember = task.Project.Members.Any(m => m.UserId == request.AssigneeId.Value);
            if (!isMember)
            {
                return Result.Failure<TaskDto>("Assignee is not a member of this project.", "INVALID_ASSIGNEE");
            }
        }

        task.AssigneeId = request.AssigneeId;

        // Reload assignee if set
        if (request.AssigneeId.HasValue)
        {
            await _dbContext.Entry(task).Reference(t => t.Assignee).LoadAsync(cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<PagedResult<TaskDto>>> GetMyTasksAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<PagedResult<TaskDto>>("User not authenticated.", "UNAUTHORIZED");
        }

        var query = _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Milestone)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Where(t => t.AssigneeId == _currentUser.UserId.Value)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var taskDtos = tasks.Select(MapToTaskDto).ToList();

        var result = new PagedResult<TaskDto>(taskDtos, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<TaskCommentDto>>> GetCommentsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            return Result.Failure<IEnumerable<TaskCommentDto>>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            return Result.Failure<IEnumerable<TaskCommentDto>>("You don't have access to this task.", "FORBIDDEN");
        }

        var comments = task.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new TaskCommentDto(
                c.Id,
                c.Content,
                c.UserId,
                $"{c.User?.FirstName} {c.User?.LastName}".Trim(),
                c.CreatedAt)).ToList();

        return Result.Success<IEnumerable<TaskCommentDto>>(comments);
    }

    public async Task<Result<TaskCommentDto>> AddCommentAsync(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            return Result.Failure<TaskCommentDto>("User not authenticated.", "UNAUTHORIZED");
        }

        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskCommentDto>("Task not found.", "NOT_FOUND");
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            return Result.Failure<TaskCommentDto>("You don't have access to this task.", "FORBIDDEN");
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            TaskId = taskId,
            UserId = _currentUser.UserId.Value,
            TenantId = _currentUser.TenantId.Value
        };

        _dbContext.Set<TaskComment>().Add(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new TaskCommentDto(
            comment.Id,
            comment.Content,
            comment.UserId,
            _currentUser.Email ?? "Unknown",
            comment.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result> DeleteCommentAsync(
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.Set<TaskComment>()
            .Include(c => c.Task)
                .ThenInclude(t => t.Project)
                    .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            return Result.Failure("Comment not found.", "NOT_FOUND");
        }

        // Only comment author or project manager+ can delete
        var isAuthor = comment.UserId == _currentUser.UserId;
        var isManager = HasProjectPermission(comment.Task.Project, _currentUser.UserId, ProjectMemberRole.Manager);

        if (!isAuthor && !isManager)
        {
            return Result.Failure("You don't have permission to delete this comment.", "FORBIDDEN");
        }

        _dbContext.Set<TaskComment>().Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static TaskDto MapToTaskDto(TaskItem task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.EstimatedHours,
            task.ActualHours,
            task.ProjectId,
            task.Project?.Name ?? "Unknown",
            task.AssigneeId,
            task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
            task.MilestoneId,
            task.Milestone?.Name,
            task.ParentTaskId,
            task.SubTasks?.Count ?? 0,
            task.Comments?.Count ?? 0,
            task.CreatedAt);
    }

    private static bool HasProjectAccess(Project project, Guid? userId)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        return project.Members.Any(m => m.UserId == userId.Value);
    }

    private static bool HasProjectPermission(Project project, Guid? userId, ProjectMemberRole minimumRole)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId.Value);
        return member != null && member.Role >= minimumRole;
    }
}
