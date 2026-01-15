using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Users.DTOs;

namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for managing users within a tenant, including CRUD operations and role assignments.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a paginated list of users for the current tenant.
    /// </summary>
    /// <param name="request">Pagination parameters including page number and page size.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing a paginated list of user list DTOs.</returns>
    Task<Result<PagedResult<UserListDto>>> GetUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a specific user by their identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the user detail DTO with full user information.</returns>
    Task<Result<UserDetailDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user within the current tenant.
    /// </summary>
    /// <param name="request">User creation details including email, name, and initial settings.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the created user list DTO.</returns>
    Task<Result<UserListDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">Updated user details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing the updated user list DTO.</returns>
    Task<Result<UserListDto>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a user by their identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns one or more roles to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">Request containing the list of role identifiers to assign.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the role assignment.</returns>
    Task<Result> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default);
}
