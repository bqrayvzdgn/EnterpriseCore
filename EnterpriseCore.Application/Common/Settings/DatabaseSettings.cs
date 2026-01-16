using System.ComponentModel.DataAnnotations;

namespace EnterpriseCore.Application.Common.Settings;

public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";

    [Required(ErrorMessage = "Database connection string is required")]
    public string DefaultConnection { get; set; } = string.Empty;

    public string? Redis { get; set; }
}
