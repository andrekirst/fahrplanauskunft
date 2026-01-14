using Fahrplanauskunft.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessageAndDefaultErrorCode()
    {
        // Arrange & Act
        var exception = new DomainException("Test error message");

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().Be("DOMAIN_ERROR");
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_ShouldSetBoth()
    {
        // Arrange & Act
        var exception = new DomainException("Test error message", "CUSTOM_CODE");

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().Be("CUSTOM_CODE");
    }

    [Fact]
    public void Constructor_WithNullErrorCode_ShouldUseDefaultErrorCode()
    {
        // Arrange & Act
        var exception = new DomainException("Test error message", (string)null!);

        // Assert
        exception.ErrorCode.Should().Be("DOMAIN_ERROR");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetBoth()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DomainException("Outer error message", innerException);

        // Assert
        exception.Message.Should().Be("Outer error message");
        exception.InnerException.Should().Be(innerException);
        exception.ErrorCode.Should().Be("DOMAIN_ERROR");
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAll()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DomainException("Outer error message", "CUSTOM_CODE", innerException);

        // Assert
        exception.Message.Should().Be("Outer error message");
        exception.ErrorCode.Should().Be("CUSTOM_CODE");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithAllParametersAndNullErrorCode_ShouldUseDefault()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DomainException("Outer error message", null!, innerException);

        // Assert
        exception.ErrorCode.Should().Be("DOMAIN_ERROR");
    }
}

public class DuplicateSequenceExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetSequenceNumberAndMessage()
    {
        // Arrange & Act
        var exception = new DuplicateSequenceException(5);

        // Assert
        exception.DuplicateSequenceNumber.Should().Be(5);
        exception.Message.Should().Contain("5");
        exception.ErrorCode.Should().Be("DUPLICATE_SEQUENCE");
    }

    [Fact]
    public void Constructor_WithZeroSequence_ShouldWork()
    {
        // Arrange & Act
        var exception = new DuplicateSequenceException(0);

        // Assert
        exception.DuplicateSequenceNumber.Should().Be(0);
    }

    [Fact]
    public void Exception_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new DuplicateSequenceException(1);

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }
}

public class InvalidStopTimeExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetMessageAndErrorCode()
    {
        // Arrange & Act
        var exception = new InvalidStopTimeException("Arrival time cannot be after departure time");

        // Assert
        exception.Message.Should().Be("Arrival time cannot be after departure time");
        exception.ErrorCode.Should().Be("INVALID_STOP_TIME");
    }

    [Fact]
    public void Exception_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new InvalidStopTimeException("Test");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }
}

public class SelfLoopExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultMessageAndErrorCode()
    {
        // Arrange & Act
        var exception = new SelfLoopException();

        // Assert
        exception.Message.Should().Contain("same origin and destination");
        exception.ErrorCode.Should().Be("SELF_LOOP");
    }

    [Fact]
    public void Exception_ShouldInheritFromDomainException()
    {
        // Arrange & Act
        var exception = new SelfLoopException();

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }
}
