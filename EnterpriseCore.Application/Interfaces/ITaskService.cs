using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Tasks.DTOs;

namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for managing tasks within projects, including CRUD operations, status updates, assignments, and comments.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Retrieves a paginated list of tasks for a specific project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">Pagination parameters including page number and page size.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a paginated list of task DTOs.</returns>
    Task<Result<PagedResult<TaskDto>>> GetTasksByProjectAsync(Guid projectId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a specific task by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the task detail DTO with full task information.</returns>
    Task<Result<TaskDetailDto>> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task within a specific project.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project to add the task to.</param>
    /// <param name="request">Task creation details including title, description, and priority.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the created task DTO.</returns>
    Task<Result<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task's information.
    /// </summary>
    /// <param name="id">The unique identifier of the task to update.</param>
    /// <param name="request">Updated task details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated task DTO.</returns>
    Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a task by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a task (e.g., from pending to in progress to completed).
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="request">Request containing the new status.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated task DTO.</returns>
    Task<Result<TaskDto>> UpdateStatusAsync(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to a specific user.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="request">Request containing the assignee's user identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated task DTO with assignment information.</returns>
    Task<Result<TaskDto>> AssignTaskAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of tasks assigned to the current user.
    /// </summary>
    /// <param name="request">Pagination parameters including page number and page size.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a paginated list of task DTOs assigned to the current user.</returns>
    Task<Result<PagedResult<TaskDto>>> GetMyTasksAsync(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all comments for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a collection of task comment DTOs.</returns>
    Task<Result<IEnumerable<TaskCommentDto>>> GetCommentsAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new comment to a task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">Comment creation details including the comment content.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the created task comment DTO.</returns>
    Task<Result<TaskCommentDto>> AddCommentAsync(Guid taskId, CreateCommentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment from a task.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
}
