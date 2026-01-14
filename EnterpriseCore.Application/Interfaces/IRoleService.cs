using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IRoleService
{
    Task<Result<PagedResult<RoleListDto>>> GetRolesAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<RoleDetailDto>> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<RoleListDto>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<RoleListDto>> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> AssignPermissionsAsync(Guid roleId, AssignPermissionsRequest request, CancellationToken cancellationToken = default);
}
