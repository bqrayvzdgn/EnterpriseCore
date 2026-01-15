using EnterpriseCore.Application.Features.Attachments.DTOs;
using EnterpriseCore.Application.Features.Attachments.Validators;
using FluentValidation.TestHelper;

namespace EnterpriseCore.Tests.Features.Validators;

public class UploadAttachmentRequestValidatorTests
{
    private readonly UploadAttachmentRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Valid_Request_With_TaskId()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024 * 1024, // 1 MB
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Valid_Request_With_ProjectId()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "image.png",
            ContentType = "image/png",
            FileSize = 500 * 1024, // 500 KB
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_FileName_Is_Empty()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "",
            ContentType = "application/pdf",
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void Should_Fail_When_FileName_Exceeds_MaxLength()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = new string('a', 256) + ".pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("File name cannot exceed 255 characters.");
    }

    [Theory]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".webp", "image/webp")]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".doc", "application/msword")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(".xls", "application/vnd.ms-excel")]
    [InlineData(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(".ppt", "application/vnd.ms-powerpoint")]
    [InlineData(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".csv", "text/csv")]
    [InlineData(".zip", "application/zip")]
    [InlineData(".rar", "application/x-rar-compressed")]
    public void Should_Pass_For_All_Allowed_Extensions_And_ContentTypes(string extension, string contentType)
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = $"file{extension}",
            ContentType = contentType,
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Extension_Is_Not_Allowed()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "malware.exe",
            ContentType = "application/pdf", // valid content type but invalid extension
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void Should_Fail_When_ContentType_Is_Not_Allowed()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "application/octet-stream", // not in allowed list
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
            .WithErrorMessage("Invalid or unsupported content type.");
    }

    [Fact]
    public void Should_Fail_When_ContentType_Is_Empty()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "",
            FileSize = 1024,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    [Fact]
    public void Should_Fail_When_FileSize_Is_Zero()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            FileSize = 0,
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSize)
            .WithErrorMessage("File size must be greater than 0.");
    }

    [Fact]
    public void Should_Fail_When_FileSize_Exceeds_Maximum()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "large-file.pdf",
            ContentType = "application/pdf",
            FileSize = 51 * 1024 * 1024, // 51 MB
            TaskId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSize)
            .WithErrorMessage("File size cannot exceed 50 MB.");
    }

    [Fact]
    public void Should_Fail_When_Neither_TaskId_Nor_ProjectId_Is_Provided()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            TaskId = null,
            ProjectId = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("File must be attached to either a Task or a Project.");
    }

    [Fact]
    public void Should_Pass_When_Both_TaskId_And_ProjectId_Are_Provided()
    {
        // Arrange
        var request = new UploadAttachmentRequest
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            TaskId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }
}
