using EnterpriseCore.Application.Features.Sprints.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Sprints.Validators;

public class CreateSprintRequestValidator : AbstractValidator<CreateSprintRequest>
{
    public CreateSprintRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sprint name is required")
            .MaximumLength(200).WithMessage("Sprint name must not exceed 200 characters");

        RuleFor(x => x.Goal)
            .MaximumLength(1000).WithMessage("Sprint goal must not exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}

public class UpdateSprintRequestValidator : AbstractValidator<UpdateSprintRequest>
{
    public UpdateSprintRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sprint name is required")
            .MaximumLength(200).WithMessage("Sprint name must not exceed 200 characters");

        RuleFor(x => x.Goal)
            .MaximumLength(1000).WithMessage("Sprint goal must not exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}
