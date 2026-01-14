using EnterpriseCore.Application.Features.Tasks.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Tasks.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(300).WithMessage("Task title cannot exceed 300 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be greater than 0.");

        RuleFor(x => x.ActualHours)
            .GreaterThanOrEqualTo(0).When(x => x.ActualHours.HasValue)
            .WithMessage("Actual hours cannot be negative.");
    }
}
