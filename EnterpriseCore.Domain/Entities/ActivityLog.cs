using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

/// <summary>
/// Immutable audit log entity - should never be updated or deleted.
/// Does not implement ISoftDeletable intentionally.
/// </summary>
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

    // FK - The user who performed the action
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
