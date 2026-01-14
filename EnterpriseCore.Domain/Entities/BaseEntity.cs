using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public abstract class BaseEntity : IAuditable, ISoftDeletable
{
    public Guid Id { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; }
}
