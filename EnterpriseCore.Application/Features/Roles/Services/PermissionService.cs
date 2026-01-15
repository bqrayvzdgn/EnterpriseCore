using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Roles.Services;

public class PermissionService : IPermissionService
{
    private readonly DbContext _dbContext;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string AllPermissionsCacheKey = "permissions:all";

    public PermissionService(DbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var cachedPermissions = await _cacheService.GetOrSetAsync(
            AllPermissionsCacheKey,
            async () =>
            {
                var permissions = await _dbContext.Set<Permission>()
                    .OrderBy(p => p.Code)
                    .AsNoTracking()
                    .Select(p => new PermissionDto(
                        p.Id,
                        p.Name,
                        p.Code,
                        p.Description))
                    .ToListAsync(cancellationToken);

                return permissions;
            },
            CacheDuration,
            cancellationToken);

        return Result.Success<IEnumerable<PermissionDto>>(cachedPermissions ?? new List<PermissionDto>());
    }
}
