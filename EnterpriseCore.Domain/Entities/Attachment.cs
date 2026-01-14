using EnterpriseCore.Domain.Interfaces;

namespace EnterpriseCore.Domain.Entities;

public class Attachment : BaseEntity, IMultiTenant
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;

    // Multi-tenancy
    public Guid TenantId { get; set; }

    // Optional relationships - can be attached to either a Task or a Project
    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Who uploaded the file
    public Guid UploadedById { get; set; }
    public User? UploadedBy { get; set; }
}
