using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.ActivityLogs.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ActivityLogsController : BaseController
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get activity logs (paginated)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityLogs(
        [FromQuery] ActivityLogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _activityLogService.GetActivityLogsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get activity logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var result = await _activityLogService.GetByEntityAsync(entityType, entityId, cancellationToken);
        return HandleResult(result);
    }
}
