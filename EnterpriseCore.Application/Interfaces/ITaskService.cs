using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Tasks.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface ITaskService
{
    Task<Result<PagedResult<TaskDto>>> GetTasksByProjectAsync(Guid projectId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<TaskDetailDto>> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> UpdateStatusAsync(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<TaskDto>> AssignTaskAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<TaskDto>>> GetMyTasksAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TaskCommentDto>>> GetCommentsAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<TaskCommentDto>> AddCommentAsync(Guid taskId, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
}
