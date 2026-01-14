using Microsoft.AspNetCore.Authorization;

namespace EnterpriseCore.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get permissions from claims
        var permissionClaims = context.User.FindAll("permissions");
        var permissions = permissionClaims.Select(c => c.Value).ToList();

        // Also check for a single permissions claim that might be comma-separated
        var singlePermissionClaim = context.User.FindFirst("permissions")?.Value;
        if (!string.IsNullOrEmpty(singlePermissionClaim))
        {
            permissions.AddRange(singlePermissionClaim.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
