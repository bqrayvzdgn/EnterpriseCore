using EnterpriseCore.Domain.Entities;

namespace EnterpriseCore.Domain.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
