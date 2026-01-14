using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class TaskComment : BaseEntity, IMultiTenant
{
    public string Content { get; set; } = string.Empty;

    // IMultiTenant
    public Guid TenantId { get; set; }

    // FKs
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual TaskItem Task { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
