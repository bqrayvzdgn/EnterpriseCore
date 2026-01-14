using System.Text.Json;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.ActivityLogs.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.ActivityLogs.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ActivityLogService(DbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Result<PagedList<ActivityLogDto>>> GetActivityLogsAsync(
        ActivityLogQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ActivityLog>()
            .Include(a => a.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= request.ToDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                CreatedAt = a.CreatedAt,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null
            })
            .ToListAsync(cancellationToken);

        var result = new PagedList<ActivityLogDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<ActivityLogDto>>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.Set<ActivityLog>()
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                CreatedAt = a.CreatedAt,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : null
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ActivityLogDto>>(logs);
    }

    public async Task LogActivityAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, _jsonOptions) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues, _jsonOptions) : null,
            UserId = _currentUserService.UserId ?? Guid.Empty,
            TenantId = _currentUserService.TenantId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Set<ActivityLog>().AddAsync(activityLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
