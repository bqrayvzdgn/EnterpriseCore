namespace EnterpriseCore.Tests.Fixtures;

public class MockCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
    public bool IsAuthenticated { get; set; }

    public MockCurrentUserService()
    {
        UserId = TestDataFactory.DefaultUserId;
        TenantId = TestDataFactory.DefaultTenantId;
        Email = "test@example.com";
        IsAuthenticated = true;
    }

    public static MockCurrentUserService CreateAuthenticated(Guid? userId = null, Guid? tenantId = null)
    {
        return new MockCurrentUserService
        {
            UserId = userId ?? TestDataFactory.DefaultUserId,
            TenantId = tenantId ?? TestDataFactory.DefaultTenantId,
            Email = "test@example.com",
            IsAuthenticated = true
        };
    }

    public static MockCurrentUserService CreateUnauthenticated()
    {
        return new MockCurrentUserService
        {
            UserId = null,
            TenantId = null,
            Email = null,
            IsAuthenticated = false
        };
    }
}
