using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class Sprint : BaseEntity, IMultiTenant
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planning;

    // Multi-tenancy
    public Guid TenantId { get; set; }

    // Project relationship
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    // Tasks in this sprint
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
