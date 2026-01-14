using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Projects.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
public class ProjectsController : BaseController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Get all projects (paginated)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateProjectAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a project
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateProjectAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteProjectAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetProjectStats(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectStatsAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a member to project
    /// </summary>
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.AddMemberAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a member from project
    /// </summary>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _projectService.RemoveMemberAsync(id, userId, cancellationToken);
        return HandleResult(result);
    }
}
