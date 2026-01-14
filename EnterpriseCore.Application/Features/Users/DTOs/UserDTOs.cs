namespace EnterpriseCore.Application.Features.Users.DTOs;

public record UserListDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IEnumerable<string> Roles,
    DateTime CreatedAt);

public record UserDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IEnumerable<RoleDto> Roles,
    int ProjectCount,
    int AssignedTaskCount,
    DateTime CreatedAt);

public record RoleDto(
    Guid Id,
    string Name,
    string? Description);

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    bool IsActive);

public record AssignRolesRequest(
    IEnumerable<Guid> RoleIds);
