using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("projects/{projectId:guid}/report")]
    public async Task<IActionResult> GetProjectReport(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetProjectReportAsync(projectId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("my-stats")]
    public async Task<IActionResult> GetMyStats(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetMyStatsAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("team-workload")]
    public async Task<IActionResult> GetTeamWorkload(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetTeamWorkloadAsync(cancellationToken);
        return HandleResult(result);
    }
}
