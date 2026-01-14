namespace EnterpriseCore.Application.Features.Roles.DTOs;

public record RoleListDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    int UserCount,
    int PermissionCount);

public record RoleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    IEnumerable<PermissionDto> Permissions);

public record PermissionDto(
    Guid Id,
    string Name,
    string Code,
    string? Description);

public record CreateRoleRequest(
    string Name,
    string? Description);

public record UpdateRoleRequest(
    string Name,
    string? Description);

public record AssignPermissionsRequest(
    IEnumerable<Guid> PermissionIds);
