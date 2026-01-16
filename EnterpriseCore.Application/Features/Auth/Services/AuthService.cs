using EnterpriseCore.Application.Common.Constants;
using EnterpriseCore.Application.Common.Models;
using EnterpriseCore.Application.Common.Settings;
using EnterpriseCore.Application.Features.Auth.DTOs;
using EnterpriseCore.Application.Interfaces;
using EnterpriseCore.Domain.Entities;
using EnterpriseCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseCore.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registration attempt for email: {Email}, tenant: {TenantName}",
            request.Email, request.TenantName);

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Registration failed: Email already exists - {Email}", request.Email);
            return Result.Failure<AuthResponse>("Email already registered.", ErrorCodes.EmailExists);
        }

        // Create tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            Slug = GenerateSlug(request.TenantName),
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant, cancellationToken);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = tenant.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // Assign Admin role
        var adminRole = await _roleRepository.GetSystemRoleByNameAsync("Admin", cancellationToken);
        if (adminRole != null)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Generate tokens
        var permissions = await _userRepository.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during user registration for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>("Registration failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("User registered successfully. UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            user.Id, user.Email, tenant.Id);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid credentials for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>("Invalid email or password.", ErrorCodes.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Account inactive for UserId: {UserId}, Email: {Email}",
                user.Id, user.Email);
            return Result.Failure<AuthResponse>("Account is deactivated.", ErrorCodes.AccountInactive);
        }

        // Generate tokens
        var permissions = await _userRepository.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during login for UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>("Login failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during login for UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>("Login failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("User logged in successfully. UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            user.Id, user.Email, user.TenantId);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
            return Result.Failure<AuthResponse>("Invalid or expired refresh token.", ErrorCodes.InvalidToken);
        }

        // Generate new tokens with rotation (invalidate old token)
        var permissions = await _userRepository.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error during token refresh for UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>("Token refresh failed. Please try again.", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during token refresh for UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>("Token refresh failed due to a database error.", ErrorCodes.DatabaseError);
        }

        _logger.LogInformation("Token refreshed successfully for UserId: {UserId}", user.Id);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes));
    }

    public Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, send email with reset token
        return Task.FromResult(Result.Success());
    }

    public Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, validate token and update password
        return Task.FromResult(Result.Success());
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            + "-" + Guid.NewGuid().ToString("N")[..8];
    }
}
