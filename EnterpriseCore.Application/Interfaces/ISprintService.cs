using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Sprints.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface ISprintService
{
    Task<Result<IReadOnlyList<SprintDto>>> GetSprintsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<SprintDto>> GetByIdAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<Result<SprintDto>> CreateAsync(Guid projectId, CreateSprintRequest request, CancellationToken cancellationToken = default);
    Task<Result<SprintDto>> UpdateAsync(Guid sprintId, UpdateSprintRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<Result<bool>> AddTaskToSprintAsync(Guid sprintId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveTaskFromSprintAsync(Guid sprintId, Guid taskId, CancellationToken cancellationToken = default);
    Task<Result<SprintDto>> StartSprintAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<Result<SprintDto>> CompleteSprintAsync(Guid sprintId, CancellationToken cancellationToken = default);
}
