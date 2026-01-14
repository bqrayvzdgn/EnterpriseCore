using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Domain.Entities;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
