using EnterpriseCore.Application.Features.Roles.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Roles.Validators;

public class AssignPermissionsRequestValidator : AbstractValidator<AssignPermissionsRequest>
{
    public AssignPermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permission IDs are required.");

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Permission ID cannot be empty.");
    }
}
