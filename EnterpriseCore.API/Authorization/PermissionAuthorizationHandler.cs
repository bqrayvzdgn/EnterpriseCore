using Microsoft.AspNetCore.Authorization;

namespace EnterpriseCore.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get permissions from claims - TokenService uses "permission" (singular) for each permission
        var permissionClaims = context.User.FindAll("permission");
        var permissions = permissionClaims.Select(c => c.Value).ToList();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
