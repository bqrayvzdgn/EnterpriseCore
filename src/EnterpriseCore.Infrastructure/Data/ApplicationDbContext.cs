using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCore.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly Guid? _currentTenantId;
    private readonly Guid? _currentUserId;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        Guid? currentTenantId,
        Guid? currentUserId) : base(options)
    {
        _currentTenantId = currentTenantId;
        _currentUserId = currentUserId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters
        ConfigureGlobalFilters(modelBuilder);
    }

    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Soft delete filter for BaseEntity types
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted && (_currentTenantId == null || e.TenantId == _currentTenantId));
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted && (_currentTenantId == null || e.TenantId == _currentTenantId));
        modelBuilder.Entity<Milestone>().HasQueryFilter(e => !e.IsDeleted && (_currentTenantId == null || e.TenantId == _currentTenantId));
        modelBuilder.Entity<TaskItem>().HasQueryFilter(e => !e.IsDeleted && (_currentTenantId == null || e.TenantId == _currentTenantId));
        modelBuilder.Entity<TaskComment>().HasQueryFilter(e => !e.IsDeleted && (_currentTenantId == null || e.TenantId == _currentTenantId));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedById = _currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedById = _currentUserId;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedById = _currentUserId;
                    break;
            }
        }

        // Set TenantId for multi-tenant entities
        foreach (var entry in ChangeTracker.Entries<IMultiTenant>())
        {
            if (entry.State == EntityState.Added && _currentTenantId.HasValue)
            {
                entry.Entity.TenantId = _currentTenantId.Value;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
