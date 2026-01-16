using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Projects.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<ProjectService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ProjectDto>>> GetProjectsAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Project>()
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var projectDtos = projects.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.Status,
            p.StartDate,
            p.EndDate,
            p.Budget,
            p.OwnerId,
            $"{p.Owner?.FirstName} {p.Owner?.LastName}".Trim(),
            p.Members.Count,
            p.Tasks.Count,
            p.Tasks.Count(t => t.Status == TaskItemStatus.Done),
            p.CreatedAt)).ToList();

        var result = new PagedResult<ProjectDto>(projectDtos, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<ProjectDetailDto>> GetProjectByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Milestones)
                .ThenInclude(m => m.Tasks)
            .Include(p => p.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project not found. ProjectId: {ProjectId}", id);
            return Result.Failure<ProjectDetailDto>("Project not found.", ErrorCodes.NotFound);
        }

        var memberDtos = project.Members.Select(m => new ProjectMemberDto(
            m.UserId,
            m.User?.Email ?? "",
            $"{m.User?.FirstName} {m.User?.LastName}".Trim(),
            m.Role,
            m.JoinedAt)).ToList();

        var milestoneDtos = project.Milestones.Select(m => new MilestoneDto(
            m.Id,
            m.Name,
            m.Description,
            m.DueDate,
            m.CompletedDate,
            m.Tasks.Count,
            m.Tasks.Count(t => t.Status == TaskItemStatus.Done))).ToList();

        var totalTasks = project.Tasks.Count;
        var completedTasks = project.Tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgressTasks = project.Tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var overdueTasks = project.Tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done);
        var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

        var stats = new ProjectStatsDto(
            totalTasks,
            completedTasks,
            inProgressTasks,
            overdueTasks,
            Math.Round(completionPercentage, 2));

        var dto = new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.Budget,
            project.OwnerId,
            $"{project.Owner?.FirstName} {project.Owner?.LastName}".Trim(),
            memberDtos,
            milestoneDtos,
            stats,
            project.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating project. Name: {ProjectName}, UserId: {UserId}",
            request.Name, _currentUser.UserId);

        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            _logger.LogWarning("Project creation failed: User not authenticated");
            return Result.Failure<ProjectDto>("User not authenticated.", ErrorCodes.Unauthorized);
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Status = ProjectStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget,
            OwnerId = _currentUser.UserId.Value,
            TenantId = _currentUser.TenantId.Value
        };

        _dbContext.Set<Project>().Add(project);

        // Add owner as project member
        var ownerMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = _currentUser.UserId.Value,
            Role = ProjectMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _dbContext.Set<ProjectMember>().Add(ownerMember);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during project creation. ProjectName: {ProjectName}", request.Name);
            return Result.Failure<ProjectDto>("Project creation failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Project created successfully. ProjectId: {ProjectId}, ProjectName: {ProjectName}, OwnerId: {OwnerId}",
            project.Id, project.Name, project.OwnerId);

        var dto = new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.Budget,
            project.OwnerId,
            _currentUser.Email ?? "Unknown",
            1,
            0,
            0,
            project.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating project. ProjectId: {ProjectId}, UserId: {UserId}",
            id, _currentUser.UserId);

        var project = await _dbContext.Set<Project>()
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project update failed: Project not found. ProjectId: {ProjectId}", id);
            return Result.Failure<ProjectDto>("Project not found.", ErrorCodes.NotFound);
        }

        // Check if user has permission to update
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            _logger.LogWarning("Project update failed: Insufficient permissions. ProjectId: {ProjectId}, UserId: {UserId}",
                id, _currentUser.UserId);
            return Result.Failure<ProjectDto>("You don't have permission to update this project.", ErrorCodes.Forbidden);
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Budget = request.Budget;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during project update. ProjectId: {ProjectId}", id);
            return Result.Failure<ProjectDto>("Project update failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during project update. ProjectId: {ProjectId}", id);
            return Result.Failure<ProjectDto>("Project update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Project updated successfully. ProjectId: {ProjectId}", project.Id);

        var dto = new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.Budget,
            project.OwnerId,
            $"{project.Owner?.FirstName} {project.Owner?.LastName}".Trim(),
            project.Members.Count,
            project.Tasks.Count,
            project.Tasks.Count(t => t.Status == TaskItemStatus.Done),
            project.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result> DeleteProjectAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project. ProjectId: {ProjectId}, UserId: {UserId}",
            id, _currentUser.UserId);

        var project = await _dbContext.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project deletion failed: Project not found. ProjectId: {ProjectId}", id);
            return Result.Failure("Project not found.", ErrorCodes.NotFound);
        }

        // Only owner can delete
        if (project.OwnerId != _currentUser.UserId)
        {
            _logger.LogWarning("Project deletion failed: Only owner can delete. ProjectId: {ProjectId}, OwnerId: {OwnerId}, UserId: {UserId}",
                id, project.OwnerId, _currentUser.UserId);
            return Result.Failure("Only the project owner can delete the project.", ErrorCodes.Forbidden);
        }

        // Soft delete - EF Core will handle this via SaveChangesAsync interceptor
        _dbContext.Set<Project>().Remove(project);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during project deletion. ProjectId: {ProjectId}", id);
            return Result.Failure("Project deletion failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Project deleted successfully. ProjectId: {ProjectId}", id);
        return Result.Success();
    }

    public async Task<Result<ProjectStatsDto>> GetProjectStatsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Tasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project stats request failed: Project not found. ProjectId: {ProjectId}", id);
            return Result.Failure<ProjectStatsDto>("Project not found.", ErrorCodes.NotFound);
        }

        var totalTasks = project.Tasks.Count;
        var completedTasks = project.Tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgressTasks = project.Tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var overdueTasks = project.Tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskItemStatus.Done);
        var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

        var stats = new ProjectStatsDto(
            totalTasks,
            completedTasks,
            inProgressTasks,
            overdueTasks,
            Math.Round(completionPercentage, 2));

        return Result.Success(stats);
    }

    public async Task<Result> AddMemberAsync(
        Guid projectId,
        AddProjectMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding member to project. ProjectId: {ProjectId}, NewMemberId: {NewMemberId}, Role: {Role}",
            projectId, request.UserId, request.Role);

        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Add member failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure("Project not found.", ErrorCodes.NotFound);
        }

        // Check if user has permission to add members
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            _logger.LogWarning("Add member failed: Insufficient permissions. ProjectId: {ProjectId}, UserId: {UserId}",
                projectId, _currentUser.UserId);
            return Result.Failure("You don't have permission to add members.", ErrorCodes.Forbidden);
        }

        // Check if user is already a member
        if (project.Members.Any(m => m.UserId == request.UserId))
        {
            _logger.LogWarning("Add member failed: User already a member. ProjectId: {ProjectId}, MemberId: {MemberId}",
                projectId, request.UserId);
            return Result.Failure("User is already a member of this project.", "ALREADY_MEMBER");
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = request.UserId,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow
        };

        _dbContext.Set<ProjectMember>().Add(member);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding member. ProjectId: {ProjectId}, MemberId: {MemberId}",
                projectId, request.UserId);
            return Result.Failure("Failed to add member due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Member added successfully. ProjectId: {ProjectId}, MemberId: {MemberId}, Role: {Role}",
            projectId, request.UserId, request.Role);
        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing member from project. ProjectId: {ProjectId}, MemberId: {MemberId}",
            projectId, userId);

        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Remove member failed: Project not found. ProjectId: {ProjectId}", projectId);
            return Result.Failure("Project not found.", ErrorCodes.NotFound);
        }

        // Can't remove the owner
        if (project.OwnerId == userId)
        {
            _logger.LogWarning("Remove member failed: Cannot remove owner. ProjectId: {ProjectId}, OwnerId: {OwnerId}",
                projectId, userId);
            return Result.Failure("Cannot remove the project owner.", "CANNOT_REMOVE_OWNER");
        }

        // Check if user has permission to remove members
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            _logger.LogWarning("Remove member failed: Insufficient permissions. ProjectId: {ProjectId}, UserId: {UserId}",
                projectId, _currentUser.UserId);
            return Result.Failure("You don't have permission to remove members.", ErrorCodes.Forbidden);
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            _logger.LogWarning("Remove member failed: User not a member. ProjectId: {ProjectId}, MemberId: {MemberId}",
                projectId, userId);
            return Result.Failure("User is not a member of this project.", ErrorCodes.NotFound);
        }

        _dbContext.Set<ProjectMember>().Remove(member);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while removing member. ProjectId: {ProjectId}, MemberId: {MemberId}",
                projectId, userId);
            return Result.Failure("Failed to remove member due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Member removed successfully. ProjectId: {ProjectId}, MemberId: {MemberId}",
            projectId, userId);
        return Result.Success();
    }

    private static bool HasProjectPermission(
        Project project,
        Guid? userId,
        ProjectMemberRole minimumRole)
    {
        if (!userId.HasValue)
        {
            return false;
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId.Value);
        return member != null && member.Role >= minimumRole;
    }
}
