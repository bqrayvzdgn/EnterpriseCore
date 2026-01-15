using EnterpriseCore.Application.Features.Sprints.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class SprintsController : BaseController
{
    private readonly ISprintService _sprintService;

    public SprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    /// <summary>
    /// Get sprint by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a sprint
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSprintRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.UpdateAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a sprint
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Start a sprint
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartSprint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.StartSprintAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Complete a sprint
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteSprint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.CompleteSprintAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a task to a sprint
    /// </summary>
    [HttpPost("{id:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTaskToSprint(
        Guid id,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.AddTaskToSprintAsync(id, taskId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a task from a sprint
    /// </summary>
    [HttpDelete("{id:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTaskFromSprint(
        Guid id,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.RemoveTaskFromSprintAsync(id, taskId, cancellationToken);
        return HandleResult(result);
    }
}

[Authorize]
[Route("api/projects/{projectId:guid}/sprints")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ProjectSprintsController : BaseController
{
    private readonly ISprintService _sprintService;

    public ProjectSprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    /// <summary>
    /// Get all sprints for a project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SprintDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetSprintsByProjectAsync(projectId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new sprint
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateSprintRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.CreateAsync(projectId, request, cancellationToken);
        return HandleResult(result);
    }
}
