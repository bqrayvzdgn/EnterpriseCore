using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Sprints.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Sprints.Services;

public class SprintService : ISprintService
{
    private readonly IRepository<Sprint> _sprintRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SprintService(
        IRepository<Sprint> sprintRepository,
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        IUnitOfWork unitOfWork)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<SprintDto>>> GetSprintsByProjectAsync(
        Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result.Failure<IReadOnlyList<SprintDto>>("Project not found", "NOT_FOUND");
        }

        var sprints = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);

        var dtos = sprints.Select(MapToDto).ToList();
        return Result.Success<IReadOnlyList<SprintDto>>(dtos);
    }

    public async Task<Result<SprintDto>> GetByIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            return Result.Failure<SprintDto>("Sprint not found", "NOT_FOUND");
        }

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> CreateAsync(
        Guid projectId,
        CreateSprintRequest request,
        CancellationToken cancellationToken = default)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            return Result.Failure<SprintDto>("Project not found", "NOT_FOUND");
        }

        if (request.EndDate <= request.StartDate)
        {
            return Result.Failure<SprintDto>("End date must be after start date", "VALIDATION_ERROR");
        }

        var sprint = new Sprint
        {
            Name = request.Name,
            Goal = request.Goal,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = SprintStatus.Planning,
            ProjectId = projectId
        };

        await _sprintRepository.AddAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> UpdateAsync(
        Guid sprintId,
        UpdateSprintRequest request,
        CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            return Result.Failure<SprintDto>("Sprint not found", "NOT_FOUND");
        }

        if (sprint.Status == SprintStatus.Completed || sprint.Status == SprintStatus.Cancelled)
        {
            return Result.Failure<SprintDto>("Cannot update completed or cancelled sprint", "VALIDATION_ERROR");
        }

        if (request.EndDate <= request.StartDate)
        {
            return Result.Failure<SprintDto>("End date must be after start date", "VALIDATION_ERROR");
        }

        sprint.Name = request.Name;
        sprint.Goal = request.Goal;
        sprint.StartDate = request.StartDate;
        sprint.EndDate = request.EndDate;

        await _sprintRepository.UpdateAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<bool>> DeleteAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.GetByIdAsync(sprintId, cancellationToken);

        if (sprint == null)
        {
            return Result.Failure<bool>("Sprint not found", "NOT_FOUND");
        }

        if (sprint.Status == SprintStatus.Active)
        {
            return Result.Failure<bool>("Cannot delete an active sprint", "VALIDATION_ERROR");
        }

        await _sprintRepository.DeleteAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> AddTaskToSprintAsync(
        Guid sprintId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.GetByIdAsync(sprintId, cancellationToken);
        if (sprint == null)
        {
            return Result.Failure<bool>("Sprint not found", "NOT_FOUND");
        }

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            return Result.Failure<bool>("Task not found", "NOT_FOUND");
        }

        if (task.ProjectId != sprint.ProjectId)
        {
            return Result.Failure<bool>("Task must belong to the same project as the sprint", "VALIDATION_ERROR");
        }

        task.SprintId = sprintId;
        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<bool>> RemoveTaskFromSprintAsync(
        Guid sprintId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            return Result.Failure<bool>("Task not found", "NOT_FOUND");
        }

        if (task.SprintId != sprintId)
        {
            return Result.Failure<bool>("Task is not in this sprint", "VALIDATION_ERROR");
        }

        task.SprintId = null;
        await _taskRepository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }

    public async Task<Result<SprintDto>> StartSprintAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            return Result.Failure<SprintDto>("Sprint not found", "NOT_FOUND");
        }

        if (sprint.Status != SprintStatus.Planning)
        {
            return Result.Failure<SprintDto>("Only sprints in planning status can be started", "VALIDATION_ERROR");
        }

        // Check if there's already an active sprint in this project
        var hasActiveSprint = await _sprintRepository.Query()
            .AnyAsync(s => s.ProjectId == sprint.ProjectId && s.Status == SprintStatus.Active, cancellationToken);

        if (hasActiveSprint)
        {
            return Result.Failure<SprintDto>("There is already an active sprint in this project", "VALIDATION_ERROR");
        }

        sprint.Status = SprintStatus.Active;
        await _sprintRepository.UpdateAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> CompleteSprintAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            return Result.Failure<SprintDto>("Sprint not found", "NOT_FOUND");
        }

        if (sprint.Status != SprintStatus.Active)
        {
            return Result.Failure<SprintDto>("Only active sprints can be completed", "VALIDATION_ERROR");
        }

        sprint.Status = SprintStatus.Completed;
        await _sprintRepository.UpdateAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(sprint));
    }

    private static SprintDto MapToDto(Sprint sprint)
    {
        return new SprintDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Goal = sprint.Goal,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status,
            ProjectId = sprint.ProjectId,
            TaskCount = sprint.Tasks?.Count ?? 0,
            CompletedTaskCount = sprint.Tasks?.Count(t => t.Status == TaskItemStatus.Done) ?? 0,
            CreatedAt = sprint.CreatedAt
        };
    }
}
