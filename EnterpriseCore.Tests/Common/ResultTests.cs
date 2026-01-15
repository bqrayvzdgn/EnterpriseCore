namespace EnterpriseCore.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        // Arrange & Act
        var result = Result.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Failure_Should_Create_Failed_Result()
    {
        // Arrange & Act
        var result = Result.Failure<string>("Error message", "ERROR_CODE");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be("Error message");
        result.ErrorCode.Should().Be("ERROR_CODE");
    }

    [Fact]
    public void Failure_With_NotFound_ErrorCode_Should_Create_NotFound_Result()
    {
        // Arrange & Act
        var result = Result.Failure<string>("Entity not found", "NOT_FOUND");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Entity not found");
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void Failure_With_Forbidden_ErrorCode_Should_Create_Forbidden_Result()
    {
        // Arrange & Act
        var result = Result.Failure<string>("Access denied", "FORBIDDEN");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Access denied");
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public void Failure_With_Unauthorized_ErrorCode_Should_Create_Unauthorized_Result()
    {
        // Arrange & Act
        var result = Result.Failure<string>("Not authorized", "UNAUTHORIZED");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Not authorized");
        result.ErrorCode.Should().Be("UNAUTHORIZED");
    }

    [Fact]
    public void Success_NonGeneric_Should_Create_Successful_Result()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_NonGeneric_Should_Create_Failed_Result()
    {
        // Arrange & Act
        var result = Result.Failure("Error occurred");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Error occurred");
    }

    [Fact]
    public void Implicit_Conversion_Should_Create_Successful_Result()
    {
        // Arrange & Act
        Result<string> result = "test value";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
    }
}
