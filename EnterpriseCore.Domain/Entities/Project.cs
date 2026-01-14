using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class Project : BaseEntity, IMultiTenant
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }

    // IMultiTenant
    public Guid TenantId { get; set; }

    // Owner
    public Guid OwnerId { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
}
