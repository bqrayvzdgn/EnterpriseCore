using EnterpriseCore.Application.Features.Dashboard.DTOs;
using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get dashboard summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get project report
    /// </summary>
    [HttpGet("projects/{projectId:guid}/report")]
    [ProducesResponseType(typeof(ProjectReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectReport(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetProjectReportAsync(projectId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current user statistics
    /// </summary>
    [HttpGet("my-stats")]
    [ProducesResponseType(typeof(MyStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStats(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetMyStatsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get team workload overview
    /// </summary>
    [HttpGet("team-workload")]
    [ProducesResponseType(typeof(TeamWorkloadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamWorkload(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetTeamWorkloadAsync(cancellationToken);
        return HandleResult(result);
    }
}
