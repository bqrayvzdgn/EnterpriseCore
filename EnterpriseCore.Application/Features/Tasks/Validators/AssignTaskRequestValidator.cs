using EnterpriseCore.Application.Features.Tasks.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Tasks.Validators;

public class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        // AssigneeId can be null (to unassign)
        // No validation needed - null is valid for unassigning
    }
}
