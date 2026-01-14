using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class TaskItem : BaseEntity, IMultiTenant
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }

    // IMultiTenant
    public Guid TenantId { get; set; }

    // FKs
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? MilestoneId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public Guid? SprintId { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User? Assignee { get; set; }
    public virtual Milestone? Milestone { get; set; }
    public virtual TaskItem? ParentTask { get; set; }
    public virtual Sprint? Sprint { get; set; }
    public virtual ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
