using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Dashboard.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<Result<ProjectReportDto>> GetProjectReportAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<MyStatsDto>> GetMyStatsAsync(CancellationToken cancellationToken = default);
    Task<Result<TeamWorkloadDto>> GetTeamWorkloadAsync(CancellationToken cancellationToken = default);
}
