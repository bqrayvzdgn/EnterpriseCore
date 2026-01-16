using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Users.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<UserListDto>>> GetUsersAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(u => new UserListDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.UserRoles.Select(ur => ur.Role.Name),
            u.CreatedAt)).ToList();

        var result = new PagedResult<UserListDto>(userDtos, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<UserDetailDto>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.ProjectMemberships)
            .Include(u => u.AssignedTasks)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", id);
            return Result.Failure<UserDetailDto>("User not found.", ErrorCodes.NotFound);
        }

        var roleDtos = user.UserRoles.Select(ur => new RoleDto(
            ur.Role.Id,
            ur.Role.Name,
            ur.Role.Description)).ToList();

        var dto = new UserDetailDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            roleDtos,
            user.ProjectMemberships.Count,
            user.AssignedTasks.Count,
            user.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<UserListDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user. Email: {Email}, TenantId: {TenantId}",
            request.Email, _currentUser.TenantId);

        if (!_currentUser.TenantId.HasValue)
        {
            _logger.LogWarning("User creation failed: User not authenticated");
            return Result.Failure<UserListDto>("User not authenticated.", ErrorCodes.Unauthorized);
        }

        // Check if email already exists in tenant
        var emailExists = await _dbContext.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning("User creation failed: Email already exists. Email: {Email}", request.Email);
            return Result.Failure<UserListDto>("Email already exists.", ErrorCodes.EmailExists);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            TenantId = _currentUser.TenantId.Value
        };

        _dbContext.Set<User>().Add(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during user creation. Email: {Email}", request.Email);
            return Result.Failure<UserListDto>("User creation failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("User created successfully. UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            user.Id, user.Email, user.TenantId);

        var dto = new UserListDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            Enumerable.Empty<string>(),
            user.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<UserListDto>> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user. UserId: {UserId}", id);

        var user = await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User update failed: User not found. UserId: {UserId}", id);
            return Result.Failure<UserListDto>("User not found.", ErrorCodes.NotFound);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during user update. UserId: {UserId}", id);
            return Result.Failure<UserListDto>("User update failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during user update. UserId: {UserId}", id);
            return Result.Failure<UserListDto>("User update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("User updated successfully. UserId: {UserId}", user.Id);

        var dto = new UserListDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name),
            user.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result> DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user. UserId: {UserId}, RequestedBy: {RequestedBy}",
            id, _currentUser.UserId);

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User deletion failed: User not found. UserId: {UserId}", id);
            return Result.Failure("User not found.", ErrorCodes.NotFound);
        }

        // Cannot delete yourself
        if (user.Id == _currentUser.UserId)
        {
            _logger.LogWarning("User deletion failed: Cannot delete self. UserId: {UserId}", id);
            return Result.Failure("Cannot delete your own account.", "CANNOT_DELETE_SELF");
        }

        _dbContext.Set<User>().Remove(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during user deletion. UserId: {UserId}", id);
            return Result.Failure("User deletion failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("User deleted successfully. UserId: {UserId}", id);
        return Result.Success();
    }

    public async Task<Result> AssignRolesAsync(
        Guid userId,
        AssignRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning roles to user. UserId: {UserId}, RoleIds: {RoleIds}",
            userId, string.Join(", ", request.RoleIds));

        var user = await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Role assignment failed: User not found. UserId: {UserId}", userId);
            return Result.Failure("User not found.", ErrorCodes.NotFound);
        }

        // Remove existing roles
        _dbContext.Set<UserRole>().RemoveRange(user.UserRoles);

        // Add new roles
        foreach (var roleId in request.RoleIds)
        {
            var roleExists = await _dbContext.Set<Role>()
                .AnyAsync(r => r.Id == roleId, cancellationToken);

            if (!roleExists)
            {
                _logger.LogWarning("Role assignment failed: Role not found. UserId: {UserId}, RoleId: {RoleId}",
                    userId, roleId);
                return Result.Failure($"Role with ID {roleId} not found.", "ROLE_NOT_FOUND");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            };

            _dbContext.Set<UserRole>().Add(userRole);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during role assignment. UserId: {UserId}", userId);
            return Result.Failure("Role assignment failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Roles assigned successfully. UserId: {UserId}, RoleCount: {RoleCount}",
            userId, request.RoleIds.Count());
        return Result.Success();
    }
}
