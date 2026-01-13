using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Settings { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
