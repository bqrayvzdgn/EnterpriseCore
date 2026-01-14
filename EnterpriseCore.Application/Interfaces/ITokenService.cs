using EnterpriseCore.Domain.Entities;

namespace EnterpriseCore.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    (Guid UserId, Guid TenantId)? ValidateToken(string token);
}
