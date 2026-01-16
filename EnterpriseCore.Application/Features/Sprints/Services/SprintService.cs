using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Sprints.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Sprints.Services;

public class SprintService : ISprintService
{
    private readonly IRepository<Sprint> _sprintRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SprintService> _logger;

    public SprintService(
        IRepository<Sprint> sprintRepository,
        IRepository<Project> projectRepository,
        IRepository<TaskItem> taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<SprintService> logger)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<SprintDto>>> GetSprintsByProjectAsync(
        Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            _logger.LogWarning("Get sprints failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure<IReadOnlyList<SprintDto>>("Project not found", ErrorCodes.NotFound);
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
            _logger.LogWarning("Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint not found", ErrorCodes.NotFound);
        }

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> CreateAsync(
        Guid projectId,
        CreateSprintRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating sprint. ProjectId: {ProjectId}, SprintName: {SprintName}",
            projectId, request.Name);

        var projectExists = await _projectRepository.ExistsAsync(projectId, cancellationToken);
        if (!projectExists)
        {
            _logger.LogWarning("Sprint creation failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure<SprintDto>("Project not found", ErrorCodes.NotFound);
        }

        if (request.EndDate <= request.StartDate)
        {
            _logger.LogWarning("Sprint creation failed: Invalid dates. StartDate: {StartDate}, EndDate: {EndDate}",
                request.StartDate, request.EndDate);
            return Result.Failure<SprintDto>("End date must be after start date", ErrorCodes.ValidationError);
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

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during sprint creation. ProjectId: {ProjectId}", projectId);
            return Result.Failure<SprintDto>("Sprint creation failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Sprint created successfully. SprintId: {SprintId}, ProjectId: {ProjectId}",
            sprint.Id, projectId);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> UpdateAsync(
        Guid sprintId,
        UpdateSprintRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating sprint. SprintId: {SprintId}", sprintId);

        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            _logger.LogWarning("Sprint update failed: Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint not found", ErrorCodes.NotFound);
        }

        if (sprint.Status == SprintStatus.Completed || sprint.Status == SprintStatus.Cancelled)
        {
            _logger.LogWarning("Sprint update failed: Invalid status. SprintId: {SprintId}, Status: {Status}",
                sprintId, sprint.Status);
            return Result.Failure<SprintDto>("Cannot update completed or cancelled sprint", ErrorCodes.ValidationError);
        }

        var newStartDate = request.StartDate ?? sprint.StartDate;
        var newEndDate = request.EndDate ?? sprint.EndDate;

        if (newEndDate <= newStartDate)
        {
            _logger.LogWarning("Sprint update failed: Invalid dates. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("End date must be after start date", ErrorCodes.ValidationError);
        }

        if (!string.IsNullOrEmpty(request.Name))
            sprint.Name = request.Name;
        if (request.Goal != null)
            sprint.Goal = request.Goal;
        sprint.StartDate = newStartDate;
        sprint.EndDate = newEndDate;

        await _sprintRepository.UpdateAsync(sprint, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during sprint update. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint update failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during sprint update. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Sprint updated successfully. SprintId: {SprintId}", sprintId);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<bool>> DeleteAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting sprint. SprintId: {SprintId}", sprintId);

        var sprint = await _sprintRepository.GetByIdAsync(sprintId, cancellationToken);

        if (sprint == null)
        {
            _logger.LogWarning("Sprint deletion failed: Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<bool>("Sprint not found", ErrorCodes.NotFound);
        }

        if (sprint.Status == SprintStatus.Active)
        {
            _logger.LogWarning("Sprint deletion failed: Sprint is active. SprintId: {SprintId}", sprintId);
            return Result.Failure<bool>("Cannot delete an active sprint", ErrorCodes.ValidationError);
        }

        await _sprintRepository.DeleteAsync(sprint, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during sprint deletion. SprintId: {SprintId}", sprintId);
            return Result.Failure<bool>("Sprint deletion failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Sprint deleted successfully. SprintId: {SprintId}", sprintId);
        return Result.Success(true);
    }

    public async Task<Result<bool>> AddTaskToSprintAsync(
        Guid sprintId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding task to sprint. SprintId: {SprintId}, TaskId: {TaskId}", sprintId, taskId);

        var sprint = await _sprintRepository.GetByIdAsync(sprintId, cancellationToken);
        if (sprint == null)
        {
            _logger.LogWarning("Add task to sprint failed: Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<bool>("Sprint not found", ErrorCodes.NotFound);
        }

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Add task to sprint failed: Task not found. TaskId: {TaskId}", taskId);
            return Result.Failure<bool>("Task not found", ErrorCodes.NotFound);
        }

        if (task.ProjectId != sprint.ProjectId)
        {
            _logger.LogWarning("Add task to sprint failed: Project mismatch. SprintProjectId: {SprintProjectId}, TaskProjectId: {TaskProjectId}",
                sprint.ProjectId, task.ProjectId);
            return Result.Failure<bool>("Task must belong to the same project as the sprint", ErrorCodes.ValidationError);
        }

        task.SprintId = sprintId;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding task to sprint. SprintId: {SprintId}, TaskId: {TaskId}",
                sprintId, taskId);
            return Result.Failure<bool>("Failed to add task to sprint.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task added to sprint successfully. SprintId: {SprintId}, TaskId: {TaskId}", sprintId, taskId);
        return Result.Success(true);
    }

    public async Task<Result<bool>> RemoveTaskFromSprintAsync(
        Guid sprintId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing task from sprint. SprintId: {SprintId}, TaskId: {TaskId}", sprintId, taskId);

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Remove task from sprint failed: Task not found. TaskId: {TaskId}", taskId);
            return Result.Failure<bool>("Task not found", ErrorCodes.NotFound);
        }

        if (task.SprintId != sprintId)
        {
            _logger.LogWarning("Remove task from sprint failed: Task not in sprint. TaskSprintId: {TaskSprintId}, SprintId: {SprintId}",
                task.SprintId, sprintId);
            return Result.Failure<bool>("Task is not in this sprint", ErrorCodes.ValidationError);
        }

        task.SprintId = null;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while removing task from sprint. SprintId: {SprintId}, TaskId: {TaskId}",
                sprintId, taskId);
            return Result.Failure<bool>("Failed to remove task from sprint.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Task removed from sprint successfully. SprintId: {SprintId}, TaskId: {TaskId}", sprintId, taskId);
        return Result.Success(true);
    }

    public async Task<Result<SprintDto>> StartSprintAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting sprint. SprintId: {SprintId}", sprintId);

        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            _logger.LogWarning("Start sprint failed: Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint not found", ErrorCodes.NotFound);
        }

        if (sprint.Status != SprintStatus.Planning)
        {
            _logger.LogWarning("Start sprint failed: Invalid status. SprintId: {SprintId}, Status: {Status}",
                sprintId, sprint.Status);
            return Result.Failure<SprintDto>("Only sprints in planning status can be started", ErrorCodes.ValidationError);
        }

        // Check if there's already an active sprint in this project
        var hasActiveSprint = await _sprintRepository.Query()
            .AnyAsync(s => s.ProjectId == sprint.ProjectId && s.Status == SprintStatus.Active, cancellationToken);

        if (hasActiveSprint)
        {
            _logger.LogWarning("Start sprint failed: Active sprint exists. SprintId: {SprintId}, ProjectId: {ProjectId}",
                sprintId, sprint.ProjectId);
            return Result.Failure<SprintDto>("There is already an active sprint in this project", ErrorCodes.ValidationError);
        }

        sprint.Status = SprintStatus.Active;
        await _sprintRepository.UpdateAsync(sprint, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while starting sprint. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Failed to start sprint.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Sprint started successfully. SprintId: {SprintId}, ProjectId: {ProjectId}",
            sprintId, sprint.ProjectId);

        return Result.Success(MapToDto(sprint));
    }

    public async Task<Result<SprintDto>> CompleteSprintAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing sprint. SprintId: {SprintId}", sprintId);

        var sprint = await _sprintRepository.Query()
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint == null)
        {
            _logger.LogWarning("Complete sprint failed: Sprint not found. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Sprint not found", ErrorCodes.NotFound);
        }

        if (sprint.Status != SprintStatus.Active)
        {
            _logger.LogWarning("Complete sprint failed: Invalid status. SprintId: {SprintId}, Status: {Status}",
                sprintId, sprint.Status);
            return Result.Failure<SprintDto>("Only active sprints can be completed", ErrorCodes.ValidationError);
        }

        sprint.Status = SprintStatus.Completed;
        await _sprintRepository.UpdateAsync(sprint, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while completing sprint. SprintId: {SprintId}", sprintId);
            return Result.Failure<SprintDto>("Failed to complete sprint.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Sprint completed successfully. SprintId: {SprintId}, ProjectId: {ProjectId}",
            sprintId, sprint.ProjectId);

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
