namespace EnterpriseCore.Domain.Interfaces;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}
