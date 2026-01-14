using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.ActivityLogs.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IActivityLogService
{
    Task<Result<PagedList<ActivityLogDto>>> GetActivityLogsAsync(
        ActivityLogQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ActivityLogDto>>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    Task LogActivityAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}
