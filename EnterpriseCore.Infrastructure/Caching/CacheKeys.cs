namespace EnterpriseCore.Infrastructure.Caching;

/// <summary>
/// Cache key patterns for consistent cache key generation
/// </summary>
public static class CacheKeys
{
    // User-related cache keys
    public static string UserPermissions(Guid userId) => $"permissions:user:{userId}";
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";

    // Project-related cache keys
    public static string Project(Guid projectId) => $"project:{projectId}";
    public static string ProjectStats(Guid projectId) => $"project:stats:{projectId}";
    public static string ProjectMembers(Guid projectId) => $"project:members:{projectId}";

    // Task-related cache keys
    public static string Task(Guid taskId) => $"task:{taskId}";

    // Role-related cache keys
    public static string Role(Guid roleId) => $"role:{roleId}";
    public static string RolePermissions(Guid roleId) => $"role:permissions:{roleId}";

    // Prefix patterns for bulk invalidation
    public static class Prefixes
    {
        public const string UserPermissions = "permissions:user:";
        public const string UserProfile = "user:profile:";
        public const string Project = "project:";
        public const string Task = "task:";
        public const string Role = "role:";
    }

    // Default TTL values
    public static class TTL
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan Long = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(1);
    }
}
