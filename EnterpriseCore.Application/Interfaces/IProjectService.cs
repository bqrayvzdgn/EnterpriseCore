using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Projects.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IProjectService
{
    Task<Result<PagedResult<ProjectDto>>> GetProjectsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProjectDetailDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProjectStatsDto>> GetProjectStatsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
