using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IPermissionService
{
    Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
