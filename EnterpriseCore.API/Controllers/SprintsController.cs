using EnterpriseCore.Application.Features.Sprints.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
public class SprintsController : BaseController
{
    private readonly ISprintService _sprintService;

    public SprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSprintRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.UpdateAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartSprint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.StartSprintAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteSprint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.CompleteSprintAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> AddTaskToSprint(
        Guid id,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.AddTaskToSprintAsync(id, taskId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}/tasks/{taskId:guid}")]
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
public class ProjectSprintsController : BaseController
{
    private readonly ISprintService _sprintService;

    public ProjectSprintsController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetSprintsByProjectAsync(projectId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateSprintRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sprintService.CreateAsync(projectId, request, cancellationToken);
        return HandleResult(result);
    }
}
