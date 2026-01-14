using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class ActivityLog : IMultiTenant
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // IMultiTenant
    public Guid TenantId { get; set; }

    // FK
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
