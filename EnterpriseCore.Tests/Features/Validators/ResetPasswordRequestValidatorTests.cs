using EnterpriseCore.Application.Features.Auth.DTOs;
using EnterpriseCore.Application.Features.Auth.Validators;
using FluentValidation.TestHelper;

namespace EnterpriseCore.Tests.Features.Validators;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Valid_Request()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "ValidP@ssw0rd!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Token_Is_Empty()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "",
            NewPassword: "ValidP@ssw0rd!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Should_Fail_When_Token_Is_Too_Short()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "short",
            NewPassword: "ValidP@ssw0rd!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Invalid reset token format.");
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Too_Short()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "Short1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Uppercase()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "lowercase1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Lowercase()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "UPPERCASE1!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Number()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "NoNumbers!"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one number.");
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Special_Character()
    {
        // Arrange
        var request = new ResetPasswordRequest(
            Token: "valid-reset-token-12345678",
            NewPassword: "NoSpecial1"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one special character.");
    }
}
