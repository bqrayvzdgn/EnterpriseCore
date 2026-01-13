namespace EnterpriseCore.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
