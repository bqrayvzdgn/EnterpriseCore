using EnterpriseCore.Application.Features.ActivityLogs.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.ActivityLogs.Validators;

public class ActivityLogQueryRequestValidator : AbstractValidator<ActivityLogQueryRequest>
{
    private static readonly string[] AllowedEntityTypes =
    {
        "Project", "Task", "User", "Role", "Sprint", "Milestone", "Attachment"
    };

    public ActivityLogQueryRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .Must(BeValidEntityType)
            .When(x => !string.IsNullOrEmpty(x.EntityType))
            .WithMessage($"EntityType must be one of: {string.Join(", ", AllowedEntityTypes)}");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("FromDate must be before or equal to ToDate.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }

    private bool BeValidEntityType(string? entityType)
    {
        return string.IsNullOrEmpty(entityType) ||
               AllowedEntityTypes.Contains(entityType, StringComparer.OrdinalIgnoreCase);
    }
}
