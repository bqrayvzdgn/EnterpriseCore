namespace EnterpriseCore.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string TenantName,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record ForgotPasswordRequest(
    string Email);

public record ResetPasswordRequest(
    string Token,
    string NewPassword);

public record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IEnumerable<string> Roles);
