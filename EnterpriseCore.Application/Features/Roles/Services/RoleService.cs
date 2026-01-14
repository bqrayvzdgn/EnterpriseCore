using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Roles.Services;

public class RoleService : IRoleService
{
    private readonly DbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        DbContext dbContext,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
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
            return Result.Failure<RoleDetailDto>("Role not found.", "NOT_FOUND");
        }

        // Check access (system roles are accessible to all, tenant roles only to tenant)
        if (!role.IsSystemRole && role.TenantId != _currentUser.TenantId)
        {
            return Result.Failure<RoleDetailDto>("Role not found.", "NOT_FOUND");
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
        if (!_currentUser.TenantId.HasValue)
        {
            return Result.Failure<RoleListDto>("User not authenticated.", "UNAUTHORIZED");
        }

        // Check if role name already exists in tenant
        var nameExists = await _dbContext.Set<Role>()
            .AnyAsync(r => r.Name == request.Name && r.TenantId == _currentUser.TenantId, cancellationToken);

        if (nameExists)
        {
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var role = await _dbContext.Set<Role>()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            return Result.Failure<RoleListDto>("Role not found.", "NOT_FOUND");
        }

        // Cannot modify system roles
        if (role.IsSystemRole)
        {
            return Result.Failure<RoleListDto>("Cannot modify system roles.", "CANNOT_MODIFY_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            return Result.Failure<RoleListDto>("Role not found.", "NOT_FOUND");
        }

        role.Name = request.Name;
        role.Description = request.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var role = await _dbContext.Set<Role>()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            return Result.Failure("Role not found.", "NOT_FOUND");
        }

        // Cannot delete system roles
        if (role.IsSystemRole)
        {
            return Result.Failure("Cannot delete system roles.", "CANNOT_DELETE_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            return Result.Failure("Role not found.", "NOT_FOUND");
        }

        // Cannot delete role if users are assigned
        if (role.UserRoles.Any())
        {
            return Result.Failure("Cannot delete role with assigned users.", "ROLE_IN_USE");
        }

        _dbContext.Set<Role>().Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignPermissionsAsync(
        Guid roleId,
        AssignPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            return Result.Failure("Role not found.", "NOT_FOUND");
        }

        // Cannot modify system roles
        if (role.IsSystemRole)
        {
            return Result.Failure("Cannot modify system role permissions.", "CANNOT_MODIFY_SYSTEM_ROLE");
        }

        // Check tenant access
        if (role.TenantId != _currentUser.TenantId)
        {
            return Result.Failure("Role not found.", "NOT_FOUND");
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
                return Result.Failure($"Permission with ID {permissionId} not found.", "PERMISSION_NOT_FOUND");
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _dbContext.Set<RolePermission>().Add(rolePermission);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
