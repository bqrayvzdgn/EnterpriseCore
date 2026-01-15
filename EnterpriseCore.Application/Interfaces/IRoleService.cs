using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;

namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for managing roles within a tenant, including CRUD operations and permission assignments.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Retrieves a paginated list of roles for the current tenant.
    /// </summary>
    /// <param name="request">Pagination parameters including page number and page size.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a paginated list of role list DTOs.</returns>
    Task<Result<PagedResult<RoleListDto>>> GetRolesAsync(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a specific role by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the role detail DTO with full role information including permissions.</returns>
    Task<Result<RoleDetailDto>> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role within the current tenant.
    /// </summary>
    /// <param name="request">Role creation details including name and description.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the created role list DTO.</returns>
    Task<Result<RoleListDto>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role's information.
    /// </summary>
    /// <param name="id">The unique identifier of the role to update.</param>
    /// <param name="request">Updated role details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated role list DTO.</returns>
    Task<Result<RoleListDto>> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a role by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns one or more permissions to a role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="request">Request containing the list of permission identifiers to assign.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the permission assignment.</returns>
    Task<Result> AssignPermissionsAsync(Guid roleId, AssignPermissionsRequest request, CancellationToken cancellationToken = default);
}
