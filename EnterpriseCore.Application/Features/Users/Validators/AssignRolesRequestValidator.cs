using EnterpriseCore.Application.Features.Users.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Users.Validators;

public class AssignRolesRequestValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("Role IDs are required.");

        RuleForEach(x => x.RoleIds)
            .NotEmpty().WithMessage("Role ID cannot be empty.");
    }
}
