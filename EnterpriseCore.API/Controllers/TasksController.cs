using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Tasks.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Get tasks by project (paginated)
    /// </summary>
    [HttpGet("projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectTasks(Guid projectId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTasksByProjectAsync(projectId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost("projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(projectId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a task
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteTaskAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update task status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateStatusAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Assign task to user
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AssignTaskAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get tasks assigned to current user
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetMyTasksAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get task comments
    /// </summary>
    [HttpGet("{taskId:guid}/comments")]
    [ProducesResponseType(typeof(IEnumerable<TaskCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetCommentsAsync(taskId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add comment to task
    /// </summary>
    [HttpPost("{taskId:guid}/comments")]
    [ProducesResponseType(typeof(TaskCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(Guid taskId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AddCommentAsync(taskId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    [HttpDelete("comments/{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteCommentAsync(commentId, cancellationToken);
        return HandleResult(result);
    }
}
