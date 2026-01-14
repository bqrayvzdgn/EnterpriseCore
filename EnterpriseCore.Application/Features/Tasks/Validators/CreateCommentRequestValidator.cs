using EnterpriseCore.Application.Features.Tasks.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Tasks.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(4000).WithMessage("Comment cannot exceed 4000 characters.");
    }
}
