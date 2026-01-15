using EnterpriseCore.Domain.Enums;

namespace EnterpriseCore.Tests.Fixtures;

public static class TestDataFactory
{
    public static Guid DefaultTenantId { get; } = Guid.NewGuid();
    public static Guid DefaultUserId { get; } = Guid.NewGuid();

    public static Tenant CreateTenant(string name = "Test Tenant")
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
            SubscriptionPlan = SubscriptionPlan.Free,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(Guid? tenantId = null, string email = "test@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? DefaultTenantId,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Project CreateProject(Guid? tenantId = null, string name = "Test Project")
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? DefaultTenantId,
            Name = name,
            Description = "Test project description",
            Status = ProjectStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static TaskItem CreateTask(Guid projectId, Guid? tenantId = null, string title = "Test Task")
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? DefaultTenantId,
            ProjectId = projectId,
            Title = title,
            Description = "Test task description",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Role CreateRole(Guid? tenantId = null, string name = "Test Role")
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? DefaultTenantId,
            Name = name,
            Description = "Test role description",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Permission CreatePermission(string name = "test.permission")
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Permission for {name}",
            CreatedAt = DateTime.UtcNow
        };
    }
}
