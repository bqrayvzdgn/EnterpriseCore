using EnterpriseCore.Application.Features.Projects.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Projects.Validators;

public class AddProjectMemberRequestValidator : AbstractValidator<AddProjectMemberRequest>
{
    public AddProjectMemberRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid member role.");
    }
}
