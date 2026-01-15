using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Projects.DTOs;

namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for managing projects within a tenant, including CRUD operations, member management, and statistics.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Retrieves a paginated list of projects for the current tenant.
    /// </summary>
    /// <param name="request">Pagination parameters including page number and page size.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a paginated list of project DTOs.</returns>
    Task<Result<PagedResult<ProjectDto>>> GetProjectsAsync(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a specific project by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the project detail DTO with full project information.</returns>
    Task<Result<ProjectDetailDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project within the current tenant.
    /// </summary>
    /// <param name="request">Project creation details including name, description, and settings.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the created project DTO.</returns>
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project's information.
    /// </summary>
    /// <param name="id">The unique identifier of the project to update.</param>
    /// <param name="request">Updated project details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated project DTO.</returns>
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a project by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves statistics for a specific project including task counts and completion rates.
    /// </summary>
    /// <param name="id">The unique identifier of the project.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the project statistics DTO.</returns>
    Task<Result<ProjectStatsDto>> GetProjectStatsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user as a member to a project with a specified role.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="request">Member details including user identifier and role.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of adding the member.</returns>
    Task<Result> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from a project's member list.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="userId">The unique identifier of the user to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of removing the member.</returns>
    Task<Result> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
