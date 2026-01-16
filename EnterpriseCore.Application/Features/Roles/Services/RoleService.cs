using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCore.Application.Features.Roles.Services;

public class RoleService : IRoleService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<RoleService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<RoleListDto>>> GetRolesAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get system roles and tenant-specific roles
        var query = _dbContext.Set<Role>()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .Where(r => r.IsSystemRole || r.TenantId == _currentUser.TenantId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var roles = await query
            .OrderBy(r => r.IsSystemRole ? 0 : 1)
            .ThenBy(r => r.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var roleDtos = roles.Select(r => new RoleListDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystemRole,
            r.UserRoles.Count,
            r.RolePermissions.Count)).ToList();

        var result = new PagedResult<RoleListDto>(roleDtos, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(result);
    }

    public async Task<Result<RoleDetailDto>> GetRoleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Role not found. RoleId: {RoleId}", id);
            return Result.Failure<RoleDetailDto>("Role not found.", ErrorCodes.NotFound);
        }

        // Check access (system roles are accessible to all, tenant roles only to tenant)
        if (!role.IsSystemRole && role.TenantId != _currentUser.TenantId)
        {
            _logger.LogWarning("Role access denied. RoleId: {RoleId}, TenantId: {TenantId}", id, _currentUser.TenantId);
            return Result.Failure<RoleDetailDto>("Role not found.", ErrorCodes.NotFound);
        }

        var permissionDtos = role.RolePermissions.Select(rp => new PermissionDto(
            rp.Permission.Id,
            rp.Permission.Name,
            rp.Permission.Code,
            rp.Permission.Description)).ToList();

        var dto = new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            permissionDtos);

        return Result.Success(dto);
    }

    public async Task<Result<RoleListDto>> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating role. RoleName: {RoleName}, TenantId: {TenantId}",
            request.Name, _currentUser.TenantId);

        if (!_currentUser.TenantId.HasValue)
        {
            _logger.LogWarning("Role creation failed: User not authenticated");
            return Result.Failure<RoleListDto>("User not authenticated.", ErrorCodes.Unauthorized);
        }

        // Check if role name already exists in tenant
        var nameExists = await _dbContext.Set<Role>()
            .AnyAsync(r => r.Name == request.Name && r.TenantId == _currentUser.TenantId, cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Role creation failed: Name already exists. RoleName: {RoleName}", request.Name);
            return Result.Failure<RoleListDto>("Role name already exists.", "NAME_EXISTS");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            TenantId = _currentUser.TenantId.Value
        };

        _dbContext.Set<Role>().Add(role);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during role creation. RoleName: {RoleName}", request.Name);
            return Result.Failure<RoleListDto>("Role creation failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Role created successfully. RoleId: {RoleId}, RoleName: {RoleName}",
            role.Id, role.Name);

        var dto = new RoleListDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            0,
            0);

        return Result.Success(dto);
    }

    public async Task<Result<RoleListDto>> UpdateRoleAsync(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating role. RoleId: {RoleId}", id);

        var role = await _dbContext.Set<Role>()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Role update failed: Role not found. RoleId: {RoleId}", id);
            return Result.Failure<RoleListDto>("Role not found.", ErrorCodes.NotFound);
        }

        // Cannot modify system roles
        if (role.IsSystemRole)
        {
            _logger.LogWarning("Role update failed: Cannot modify system role. RoleId: {RoleId}", id);
            return Result.Failure<RoleListDto>("Cannot modify system roles.", "CANNOT_MODIFY_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            _logger.LogWarning("Role update failed: Tenant mismatch. RoleId: {RoleId}, TenantId: {TenantId}",
                id, _currentUser.TenantId);
            return Result.Failure<RoleListDto>("Role not found.", ErrorCodes.NotFound);
        }

        role.Name = request.Name;
        role.Description = request.Description;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during role update. RoleId: {RoleId}", id);
            return Result.Failure<RoleListDto>("Role update failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during role update. RoleId: {RoleId}", id);
            return Result.Failure<RoleListDto>("Role update failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Role updated successfully. RoleId: {RoleId}", role.Id);

        var dto = new RoleListDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.UserRoles.Count,
            role.RolePermissions.Count);

        return Result.Success(dto);
    }

    public async Task<Result> DeleteRoleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting role. RoleId: {RoleId}", id);

        var role = await _dbContext.Set<Role>()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Role deletion failed: Role not found. RoleId: {RoleId}", id);
            return Result.Failure("Role not found.", ErrorCodes.NotFound);
        }

        // Cannot delete system roles
        if (role.IsSystemRole)
        {
            _logger.LogWarning("Role deletion failed: Cannot delete system role. RoleId: {RoleId}", id);
            return Result.Failure("Cannot delete system roles.", "CANNOT_DELETE_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            _logger.LogWarning("Role deletion failed: Tenant mismatch. RoleId: {RoleId}, TenantId: {TenantId}",
                id, _currentUser.TenantId);
            return Result.Failure("Role not found.", ErrorCodes.NotFound);
        }

        // Cannot delete role if users are assigned
        if (role.UserRoles.Any())
        {
            _logger.LogWarning("Role deletion failed: Role has assigned users. RoleId: {RoleId}, UserCount: {UserCount}",
                id, role.UserRoles.Count);
            return Result.Failure("Cannot delete role with assigned users.", "ROLE_IN_USE");
        }

        _dbContext.Set<Role>().Remove(role);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during role deletion. RoleId: {RoleId}", id);
            return Result.Failure("Role deletion failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Role deleted successfully. RoleId: {RoleId}", id);
        return Result.Success();
    }

    public async Task<Result> AssignPermissionsAsync(
        Guid roleId,
        AssignPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning permissions to role. RoleId: {RoleId}, PermissionCount: {PermissionCount}",
            roleId, request.PermissionIds.Count());

        var role = await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Permission assignment failed: Role not found. RoleId: {RoleId}", roleId);
            return Result.Failure("Role not found.", ErrorCodes.NotFound);
        }

        // Cannot modify system roles
        if (role.IsSystemRole)
        {
            _logger.LogWarning("Permission assignment failed: Cannot modify system role. RoleId: {RoleId}", roleId);
            return Result.Failure("Cannot modify system role permissions.", "CANNOT_MODIFY_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            _logger.LogWarning("Permission assignment failed: Tenant mismatch. RoleId: {RoleId}, TenantId: {TenantId}",
                roleId, _currentUser.TenantId);
            return Result.Failure("Role not found.", ErrorCodes.NotFound);
        }

        // Remove existing permissions
        _dbContext.Set<RolePermission>().RemoveRange(role.RolePermissions);

        // Add new permissions
        foreach (var permissionId in request.PermissionIds)
        {
            var permissionExists = await _dbContext.Set<Permission>()
                .AnyAsync(p => p.Id == permissionId, cancellationToken);

            if (!permissionExists)
            {
                _logger.LogWarning("Permission assignment failed: Permission not found. RoleId: {RoleId}, PermissionId: {PermissionId}",
                    roleId, permissionId);
                return Result.Failure($"Permission with ID {permissionId} not found.", "PERMISSION_NOT_FOUND");
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _dbContext.Set<RolePermission>().Add(rolePermission);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during permission assignment. RoleId: {RoleId}", roleId);
            return Result.Failure("Permission assignment failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Permissions assigned successfully. RoleId: {RoleId}, PermissionCount: {PermissionCount}",
            roleId, request.PermissionIds.Count());

        return Result.Success();
    }
}
