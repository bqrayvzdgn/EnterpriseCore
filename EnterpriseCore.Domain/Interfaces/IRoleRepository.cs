using EnterpriseCore.Domain.Entities;

namespace EnterpriseCore.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetSystemRoleByNameAsync(string name, CancellationToken cancellationToken = default);
}
