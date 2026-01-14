using EnterpriseCore.Application.Features.Tasks.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Tasks.Validators;

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.");
    }
}
