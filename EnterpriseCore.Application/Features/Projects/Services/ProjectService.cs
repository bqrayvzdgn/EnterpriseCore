using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Projects.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
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
            return Result.Failure<ProjectDetailDto>("Project not found.", "NOT_FOUND");
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
        if (!_currentUser.UserId.HasValue || !_currentUser.TenantId.HasValue)
        {
            return Result.Failure<ProjectDto>("User not authenticated.", "UNAUTHORIZED");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            return Result.Failure<ProjectDto>("Project not found.", "NOT_FOUND");
        }

        // Check if user has permission to update
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            return Result.Failure<ProjectDto>("You don't have permission to update this project.", "FORBIDDEN");
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Budget = request.Budget;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var project = await _dbContext.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
        {
            return Result.Failure("Project not found.", "NOT_FOUND");
        }

        // Only owner can delete
        if (project.OwnerId != _currentUser.UserId)
        {
            return Result.Failure("Only the project owner can delete the project.", "FORBIDDEN");
        }

        // Soft delete - EF Core will handle this via SaveChangesAsync interceptor
        _dbContext.Set<Project>().Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            return Result.Failure<ProjectStatsDto>("Project not found.", "NOT_FOUND");
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
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure("Project not found.", "NOT_FOUND");
        }

        // Check if user has permission to add members
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            return Result.Failure("You don't have permission to add members.", "FORBIDDEN");
        }

        // Check if user is already a member
        if (project.Members.Any(m => m.UserId == request.UserId))
        {
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Set<Project>()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            return Result.Failure("Project not found.", "NOT_FOUND");
        }

        // Can't remove the owner
        if (project.OwnerId == userId)
        {
            return Result.Failure("Cannot remove the project owner.", "CANNOT_REMOVE_OWNER");
        }

        // Check if user has permission to remove members
        if (!HasProjectPermission(project, _currentUser.UserId, ProjectMemberRole.Manager))
        {
            return Result.Failure("You don't have permission to remove members.", "FORBIDDEN");
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            return Result.Failure("User is not a member of this project.", "NOT_FOUND");
        }

        _dbContext.Set<ProjectMember>().Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
