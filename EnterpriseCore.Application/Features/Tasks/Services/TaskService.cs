using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Tasks.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<TaskService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
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
            _logger.LogWarning("Get tasks failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure<PagedResult<TaskDto>>("Project not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectAccess(project, _currentUser.UserId))
        {
            _logger.LogWarning("Get tasks failed: Access denied. ProjectId: {ProjectId}, UserId: {UserId}",
                projectId, _currentUser.UserId);
            return Result.Failure<PagedResult<TaskDto>>("You don't have access to this project.", ErrorCodes.Forbidden);
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
            _logger.LogWarning("Task not found. TaskId: {TaskId}", id);
            return Result.Failure<TaskDetailDto>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            _logger.LogWarning("Task access denied. TaskId: {TaskId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure<TaskDetailDto>("You don't have access to this task.", ErrorCodes.Forbidden);
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
        _logger.LogInformation("Creating task. ProjectId: {ProjectId}, Title: {Title}, UserId: {UserId}",
            projectId, request.Title, _currentUser.UserId);

        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            _logger.LogWarning("Task creation failed: User not authenticated");
            return Result.Failure<TaskDto>("User not authenticated.", ErrorCodes.Unauthorized);
        }

        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Task creation failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure<TaskDto>("Project not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            _logger.LogWarning("Task creation failed: Insufficient permissions. ProjectId: {ProjectId}, UserId: {UserId}",
                projectId, _currentUser.UserId);
            return Result.Failure<TaskDto>("You don't have permission to create tasks in this project.", ErrorCodes.Forbidden);
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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during task creation. ProjectId: {ProjectId}, Title: {Title}",
                projectId, request.Title);
            return Result.Failure<TaskDto>("Task creation failed due to a database error.", ErrorCodes.DatabaseError);
        }

        // Load navigation properties for response
        await _dbContext.Entry(task).Reference(t => t.Project).LoadAsync(cancellationToken);
        if (task.AssigneeId.HasValue)
        {
            await _dbContext.Entry(task).Reference(t => t.Assignee).LoadAsync(cancellationToken);
        }

        _logger.LogInformation("Task created successfully. TaskId: {TaskId}, ProjectId: {ProjectId}, Title: {Title}",
            task.Id, projectId, task.Title);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<TaskDto>> UpdateTaskAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task. TaskId: {TaskId}, UserId: {UserId}", id, _currentUser.UserId);

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
            _logger.LogWarning("Task update failed: Task not found. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            _logger.LogWarning("Task update failed: Insufficient permissions. TaskId: {TaskId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure<TaskDto>("You don't have permission to update this task.", ErrorCodes.Forbidden);
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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during task update. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task update failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during task update. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task updated successfully. TaskId: {TaskId}", task.Id);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result> DeleteTaskAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task. TaskId: {TaskId}, UserId: {UserId}", id, _currentUser.UserId);

        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task deletion failed: Task not found. TaskId: {TaskId}", id);
            return Result.Failure("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            _logger.LogWarning("Task deletion failed: Insufficient permissions. TaskId: {TaskId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure("You don't have permission to delete this task.", ErrorCodes.Forbidden);
        }

        // Soft delete
        _dbContext.Set<TaskItem>().Remove(task);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during task deletion. TaskId: {TaskId}", id);
            return Result.Failure("Task deletion failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task deleted successfully. TaskId: {TaskId}", id);
        return Result.Success();
    }

    public async Task<Result<TaskDto>> UpdateStatusAsync(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task status. TaskId: {TaskId}, NewStatus: {NewStatus}, UserId: {UserId}",
            id, request.Status, _currentUser.UserId);

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
            _logger.LogWarning("Task status update failed: Task not found. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            _logger.LogWarning("Task status update failed: Insufficient permissions. TaskId: {TaskId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure<TaskDto>("You don't have permission to update this task.", ErrorCodes.Forbidden);
        }

        var oldStatus = task.Status;
        task.Status = request.Status;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during task status update. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task status update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task status updated. TaskId: {TaskId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            id, oldStatus, request.Status);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<TaskDto>> AssignTaskAsync(
        Guid id,
        AssignTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning task. TaskId: {TaskId}, AssigneeId: {AssigneeId}, UserId: {UserId}",
            id, request.AssigneeId, _currentUser.UserId);

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
            _logger.LogWarning("Task assignment failed: Task not found. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectPermission(task.Project, _currentUser.UserId, ProjectMemberRole.Member))
        {
            _logger.LogWarning("Task assignment failed: Insufficient permissions. TaskId: {TaskId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure<TaskDto>("You don't have permission to assign this task.", ErrorCodes.Forbidden);
        }

        // Verify assignee is a project member (if not null)
        if (request.AssigneeId.HasValue)
        {
            var isMember = task.Project.Members.Any(m => m.UserId == request.AssigneeId.Value);
            if (!isMember)
            {
                _logger.LogWarning("Task assignment failed: Invalid assignee. TaskId: {TaskId}, AssigneeId: {AssigneeId}",
                    id, request.AssigneeId);
                return Result.Failure<TaskDto>("Assignee is not a member of this project.", "INVALID_ASSIGNEE");
            }
        }

        var oldAssigneeId = task.AssigneeId;
        task.AssigneeId = request.AssigneeId;

        // Reload assignee if set
        if (request.AssigneeId.HasValue)
        {
            await _dbContext.Entry(task).Reference(t => t.Assignee).LoadAsync(cancellationToken);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during task assignment. TaskId: {TaskId}", id);
            return Result.Failure<TaskDto>("Task assignment failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task assigned successfully. TaskId: {TaskId}, OldAssigneeId: {OldAssigneeId}, NewAssigneeId: {NewAssigneeId}",
            id, oldAssigneeId, request.AssigneeId);

        var dto = MapToTaskDto(task);
        return Result.Success(dto);
    }

    public async Task<Result<PagedResult<TaskDto>>> GetMyTasksAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.UserId.HasValue)
        {
            _logger.LogWarning("Get my tasks failed: User not authenticated");
            return Result.Failure<PagedResult<TaskDto>>("User not authenticated.", ErrorCodes.Unauthorized);
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
            _logger.LogWarning("Get comments failed: Task not found. TaskId: {TaskId}", taskId);
            return Result.Failure<IEnumerable<TaskCommentDto>>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            _logger.LogWarning("Get comments failed: Access denied. TaskId: {TaskId}, UserId: {UserId}",
                taskId, _currentUser.UserId);
            return Result.Failure<IEnumerable<TaskCommentDto>>("You don't have access to this task.", ErrorCodes.Forbidden);
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
        _logger.LogInformation("Adding comment to task. TaskId: {TaskId}, UserId: {UserId}",
            taskId, _currentUser.UserId);

        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            _logger.LogWarning("Add comment failed: User not authenticated");
            return Result.Failure<TaskCommentDto>("User not authenticated.", ErrorCodes.Unauthorized);
        }

        var task = await _dbContext.Set<TaskItem>()
            .Include(t => t.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Add comment failed: Task not found. TaskId: {TaskId}", taskId);
            return Result.Failure<TaskCommentDto>("Task not found.", ErrorCodes.NotFound);
        }

        if (!HasProjectAccess(task.Project, _currentUser.UserId))
        {
            _logger.LogWarning("Add comment failed: Access denied. TaskId: {TaskId}, UserId: {UserId}",
                taskId, _currentUser.UserId);
            return Result.Failure<TaskCommentDto>("You don't have access to this task.", ErrorCodes.Forbidden);
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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding comment. TaskId: {TaskId}", taskId);
            return Result.Failure<TaskCommentDto>("Failed to add comment due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Comment added successfully. TaskId: {TaskId}, CommentId: {CommentId}",
            taskId, comment.Id);

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
        _logger.LogInformation("Deleting comment. CommentId: {CommentId}, UserId: {UserId}",
            commentId, _currentUser.UserId);

        var comment = await _dbContext.Set<TaskComment>()
            .Include(c => c.Task)
                .ThenInclude(t => t.Project)
                    .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            _logger.LogWarning("Delete comment failed: Comment not found. CommentId: {CommentId}", commentId);
            return Result.Failure("Comment not found.", ErrorCodes.NotFound);
        }

        // Only comment author or project manager+ can delete
        var isAuthor = comment.UserId == _currentUser.UserId;
        var isManager = HasProjectPermission(comment.Task.Project, _currentUser.UserId, ProjectMemberRole.Manager);

        if (!isAuthor && !isManager)
        {
            _logger.LogWarning("Delete comment failed: Insufficient permissions. CommentId: {CommentId}, UserId: {UserId}",
                commentId, _currentUser.UserId);
            return Result.Failure("You don't have permission to delete this comment.", ErrorCodes.Forbidden);
        }

        _dbContext.Set<TaskComment>().Remove(comment);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting comment. CommentId: {CommentId}", commentId);
            return Result.Failure("Failed to delete comment due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Comment deleted successfully. CommentId: {CommentId}", commentId);
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
