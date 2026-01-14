using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class Milestone : BaseEntity, IMultiTenant
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // IMultiTenant
    public Guid TenantId { get; set; }

    // Project FK
    public Guid ProjectId { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
