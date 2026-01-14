using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Application.Features.Tasks.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid ProjectId,
    string ProjectName,
    Guid? AssigneeId,
    string? AssigneeName,
    Guid? MilestoneId,
    string? MilestoneName,
    Guid? ParentTaskId,
    int SubTaskCount,
    int CommentCount,
    DateTime CreatedAt);

public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid ProjectId,
    string ProjectName,
    Guid? AssigneeId,
    string? AssigneeName,
    Guid? MilestoneId,
    string? MilestoneName,
    Guid? ParentTaskId,
    IEnumerable<TaskDto> SubTasks,
    IEnumerable<TaskCommentDto> Comments,
    DateTime CreatedAt);

public record TaskCommentDto(
    Guid Id,
    string Content,
    Guid UserId,
    string UserName,
    DateTime CreatedAt);

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    decimal? EstimatedHours,
    Guid? AssigneeId,
    Guid? MilestoneId,
    Guid? ParentTaskId);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid? AssigneeId,
    Guid? MilestoneId);

public record UpdateTaskStatusRequest(
    TaskItemStatus Status);

public record AssignTaskRequest(
    Guid? AssigneeId);

public record CreateCommentRequest(
    string Content);
