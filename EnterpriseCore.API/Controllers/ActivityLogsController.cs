using EnterpriseCore.Application.Features.ActivityLogs.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
public class ActivityLogsController : BaseController
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityLogs(
        [FromQuery] ActivityLogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _activityLogService.GetActivityLogsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var result = await _activityLogService.GetByEntityAsync(entityType, entityId, cancellationToken);
        return HandleResult(result);
    }
}
