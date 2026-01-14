using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Users.DTOs;

namespace EnterpriseCore.Application.Interfaces;

public interface IUserService
{
    Task<Result<PagedResult<UserListDto>>> GetUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDetailDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserListDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserListDto>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default);
}
