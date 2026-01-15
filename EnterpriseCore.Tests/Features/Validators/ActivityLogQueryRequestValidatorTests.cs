using EnterpriseCore.Application.Features.ActivityLogs.DTOs;
using EnterpriseCore.Application.Features.ActivityLogs.Validators;
using FluentValidation.TestHelper;

namespace EnterpriseCore.Tests.Features.Validators;

public class ActivityLogQueryRequestValidatorTests
{
    private readonly ActivityLogQueryRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Valid_Request()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            EntityType = "Project",
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_EntityType_Is_Null()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            EntityType = null,
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EntityType);
    }

    [Theory]
    [InlineData("Project")]
    [InlineData("Task")]
    [InlineData("User")]
    [InlineData("Role")]
    [InlineData("Sprint")]
    [InlineData("Milestone")]
    [InlineData("Attachment")]
    public void Should_Pass_For_All_Valid_EntityTypes(string entityType)
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            EntityType = entityType,
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EntityType);
    }

    [Fact]
    public void Should_Fail_When_EntityType_Is_Invalid()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            EntityType = "InvalidEntityType",
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EntityType);
    }

    [Fact]
    public void Should_Fail_When_FromDate_Is_After_ToDate()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            FromDate = DateTime.UtcNow.AddDays(1),
            ToDate = DateTime.UtcNow,
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FromDate)
            .WithErrorMessage("FromDate must be before or equal to ToDate.");
    }

    [Fact]
    public void Should_Pass_When_FromDate_Equals_ToDate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = new ActivityLogQueryRequest
        {
            FromDate = now,
            ToDate = now,
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FromDate);
    }

    [Fact]
    public void Should_Fail_When_PageNumber_Is_Zero()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            PageNumber = 0,
            PageSize = 20
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("PageNumber must be at least 1.");
    }

    [Fact]
    public void Should_Fail_When_PageSize_Exceeds_Maximum()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            PageNumber = 1,
            PageSize = 101
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100.");
    }

    [Fact]
    public void Should_Fail_When_PageSize_Is_Zero()
    {
        // Arrange
        var request = new ActivityLogQueryRequest
        {
            PageNumber = 1,
            PageSize = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100.");
    }
}
