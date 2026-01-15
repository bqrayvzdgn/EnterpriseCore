using EnterpriseCore.Application.Features.Attachments.DTOs;
using FluentValidation;

namespace EnterpriseCore.Application.Features.Attachments.Validators;

public class UploadAttachmentRequestValidator : AbstractValidator<UploadAttachmentRequest>
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain", "text/csv",
        "application/zip", "application/x-rar-compressed"
    };

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf",
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv",
        ".zip", ".rar"
    };

    public UploadAttachmentRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters.")
            .Must(HaveValidExtension).WithMessage($"File extension must be one of: {string.Join(", ", AllowedExtensions)}");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(BeValidContentType).WithMessage("Invalid or unsupported content type.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x)
            .Must(HaveTaskOrProject).WithMessage("File must be attached to either a Task or a Project.");
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);
    }

    private bool BeValidContentType(string contentType)
    {
        return !string.IsNullOrEmpty(contentType) &&
               AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private bool HaveTaskOrProject(UploadAttachmentRequest request)
    {
        return request.TaskId.HasValue || request.ProjectId.HasValue;
    }
}
