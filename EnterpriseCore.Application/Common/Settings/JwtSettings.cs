using System.ComponentModel.DataAnnotations;

namespace EnterpriseCore.Application.Common.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(ErrorMessage = "JWT SecretKey is required")]
    [MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters")]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "EnterpriseCore";

    [Required]
    public string Audience { get; set; } = "EnterpriseCore";

    [Range(1, 1440, ErrorMessage = "ExpirationMinutes must be between 1 and 1440")]
    public int ExpirationMinutes { get; set; } = 60;

    [Range(1, 30, ErrorMessage = "RefreshTokenExpirationDays must be between 1 and 30")]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
