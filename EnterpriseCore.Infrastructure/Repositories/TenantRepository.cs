using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using EnterpriseCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.Slug == slug, cancellationToken);
    }
}
