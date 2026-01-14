using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Roles.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Application.Features.Roles.Services;

public class PermissionService : IPermissionService
{
    private readonly DbContext _dbContext;

    public PermissionService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await _dbContext.Set<Permission>()
            .OrderBy(p => p.Code)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = permissions.Select(p => new PermissionDto(
            p.Id,
            p.Name,
            p.Code,
            p.Description)).ToList();

        return Result.Success<IEnumerable<PermissionDto>>(dtos);
    }
}
