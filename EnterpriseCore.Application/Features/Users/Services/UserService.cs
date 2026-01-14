using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Users.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
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
            return Result.Failure<UserDetailDto>("User not found.", "NOT_FOUND");
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
        if (!_currentUser.TenantId.HasValue)
        {
            return Result.Failure<UserListDto>("User not authenticated.", "UNAUTHORIZED");
        }

        // Check if email already exists in tenant
        var emailExists = await _dbContext.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<UserListDto>("Email already exists.", "EMAIL_EXISTS");
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var user = await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserListDto>("User not found.", "NOT_FOUND");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found.", "NOT_FOUND");
        }

        // Cannot delete yourself
        if (user.Id == _currentUser.UserId)
        {
            return Result.Failure("Cannot delete your own account.", "CANNOT_DELETE_SELF");
        }

        _dbContext.Set<User>().Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignRolesAsync(
        Guid userId,
        AssignRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found.", "NOT_FOUND");
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
