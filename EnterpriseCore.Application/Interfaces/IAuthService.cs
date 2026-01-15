using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Features.Auth.DTOs;

namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for authentication and authorization operations including user registration, login, and password management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and creates their tenant.
    /// </summary>
    /// <param name="request">Registration details including email, password, and user information.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing authentication response with access and refresh tokens on success.</returns>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with their credentials.
    /// </summary>
    /// <param name="request">Login credentials including email and password.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing authentication response with access and refresh tokens on success.</returns>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">Request containing the refresh token.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result containing new authentication response with fresh access and refresh tokens.</returns>
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates the password reset process by sending a reset link to the user's email.
    /// </summary>
    /// <param name="request">Request containing the user's email address.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the password reset initiation.</returns>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="request">Request containing the reset token and new password.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Result indicating success or failure of the password reset.</returns>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
