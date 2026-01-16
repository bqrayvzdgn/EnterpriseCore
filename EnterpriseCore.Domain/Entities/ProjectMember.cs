using EnterpriseCore.Domain.Enums;
using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class ProjectMember : IAuditable, ISoftDeletable
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
