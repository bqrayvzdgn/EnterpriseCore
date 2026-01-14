using EnterpriseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCore.API.Controllers;

[Authorize]
public class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        var result = await _permissionService.GetAllPermissionsAsync(cancellationToken);
        return HandleResult(result);
    }
}
