using System.Globalization;
using Fahrplanauskunft.Infrastructure.Gtfs;

namespace Fahrplanauskunft.Tests.Unit.Gtfs;

/// <summary>
/// Unit tests for the <see cref="GtfsParsingException"/> class.
/// Tests verify all constructors, properties, and message formatting.
/// </summary>
public class GtfsParsingExceptionTests
{
    #region Basic Constructor Tests

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var exception = new GtfsParsingException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
        exception.FileName.Should().BeNull();
        exception.LineNumber.Should().BeNull();
        exception.MissingFiles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBothProperties()
    {
        // Arrange
        const string message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new GtfsParsingException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
        exception.FileName.Should().BeNull();
        exception.LineNumber.Should().BeNull();
        exception.MissingFiles.Should().BeEmpty();
    }

    #endregion

    #region File Context Constructor Tests

    [Fact]
    public void Constructor_WithFileContext_ShouldSetFileNameAndLineNumber()
    {
        // Arrange
        const string message = "Invalid data";
        const string fileName = "stops.txt";
        const int lineNumber = 42;

        // Act
        var exception = new GtfsParsingException(message, fileName, lineNumber);

        // Assert
        exception.FileName.Should().Be(fileName);
        exception.LineNumber.Should().Be(lineNumber);
        exception.MissingFiles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithFileContext_ShouldFormatMessageWithLineNumber()
    {
        // Arrange
        const string message = "Invalid latitude value";
        const string fileName = "stops.txt";
        const int lineNumber = 10;

        // Act
        var exception = new GtfsParsingException(message, fileName, lineNumber);

        // Assert
        exception.Message.Should().Be("Error parsing 'stops.txt' at line 10: Invalid latitude value");
    }

    [Fact]
    public void Constructor_WithFileContext_WithoutLineNumber_ShouldFormatMessageWithoutLineNumber()
    {
        // Arrange
        const string message = "File corrupted";
        const string fileName = "routes.txt";

        // Act
        var exception = new GtfsParsingException(message, fileName);

        // Assert
        exception.Message.Should().Be("Error parsing 'routes.txt': File corrupted");
        exception.LineNumber.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFileContext_AndInnerException_ShouldSetAllProperties()
    {
        // Arrange
        const string message = "Type conversion failed";
        const string fileName = "trips.txt";
        const int lineNumber = 25;
        var innerException = new FormatException("Cannot parse integer");

        // Act
        var exception = new GtfsParsingException(message, fileName, lineNumber, innerException);

        // Assert
        exception.FileName.Should().Be(fileName);
        exception.LineNumber.Should().Be(lineNumber);
        exception.InnerException.Should().BeSameAs(innerException);
        exception.Message.Should().Contain(fileName);
        exception.Message.Should().Contain(lineNumber.ToString(CultureInfo.InvariantCulture));
    }

    #endregion

    #region Missing Files Constructor Tests

    [Fact]
    public void Constructor_WithMissingFiles_SingleFile_ShouldFormatSingularMessage()
    {
        // Arrange
        var missingFiles = new[] { "stops.txt" };

        // Act
        var exception = new GtfsParsingException(missingFiles);

        // Assert
        exception.Message.Should().Be("Required GTFS file is missing: stops.txt");
        exception.MissingFiles.Should().ContainSingle().Which.Should().Be("stops.txt");
    }

    [Fact]
    public void Constructor_WithMissingFiles_MultipleFiles_ShouldFormatPluralMessage()
    {
        // Arrange
        var missingFiles = new[] { "stops.txt", "routes.txt", "trips.txt" };

        // Act
        var exception = new GtfsParsingException(missingFiles);

        // Assert
        exception.Message.Should().Be("Required GTFS files are missing: stops.txt, routes.txt, trips.txt");
        exception.MissingFiles.Should().HaveCount(3);
        exception.MissingFiles.Should().BeEquivalentTo(missingFiles);
    }

    [Fact]
    public void Constructor_WithMissingFiles_EmptyList_ShouldFormatGenericMessage()
    {
        // Arrange
        var missingFiles = Array.Empty<string>();

        // Act
        var exception = new GtfsParsingException(missingFiles);

        // Assert
        exception.Message.Should().Be("Required GTFS files are missing.");
        exception.MissingFiles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMissingFiles_AndInnerException_ShouldSetBothProperties()
    {
        // Arrange
        var missingFiles = new[] { "calendar.txt", "stop_times.txt" };
        var innerException = new IOException("Disk error");

        // Act
        var exception = new GtfsParsingException(missingFiles, innerException);

        // Assert
        exception.MissingFiles.Should().HaveCount(2);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void Constructor_WithMissingFiles_ShouldCreateReadOnlyCollection()
    {
        // Arrange
        var missingFiles = new List<string> { "stops.txt", "routes.txt" };

        // Act
        var exception = new GtfsParsingException(missingFiles);

        // Assert - MissingFiles should be immutable
        exception.MissingFiles.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    #endregion

    #region IsMissingFilesError Property Tests

    [Fact]
    public void IsMissingFilesError_WhenMissingFilesPresent_ShouldReturnTrue()
    {
        // Arrange
        var exception = new GtfsParsingException(["stops.txt"]);

        // Act & Assert
        exception.IsMissingFilesError.Should().BeTrue();
    }

    [Fact]
    public void IsMissingFilesError_WhenNoMissingFiles_ShouldReturnFalse()
    {
        // Arrange
        var exception = new GtfsParsingException("Some error");

        // Act & Assert
        exception.IsMissingFilesError.Should().BeFalse();
    }

    [Fact]
    public void IsMissingFilesError_WithFileContext_ShouldReturnFalse()
    {
        // Arrange
        var exception = new GtfsParsingException("Error", "stops.txt", 5);

        // Act & Assert
        exception.IsMissingFilesError.Should().BeFalse();
    }

    #endregion

    #region HasFileContext Property Tests

    [Fact]
    public void HasFileContext_WhenFileNameProvided_ShouldReturnTrue()
    {
        // Arrange
        var exception = new GtfsParsingException("Error", "stops.txt");

        // Act & Assert
        exception.HasFileContext.Should().BeTrue();
    }

    [Fact]
    public void HasFileContext_WhenNoFileName_ShouldReturnFalse()
    {
        // Arrange
        var exception = new GtfsParsingException("Error");

        // Act & Assert
        exception.HasFileContext.Should().BeFalse();
    }

    [Fact]
    public void HasFileContext_WithMissingFiles_ShouldReturnFalse()
    {
        // Arrange
        var exception = new GtfsParsingException(["stops.txt"]);

        // Act & Assert
        exception.HasFileContext.Should().BeFalse();
    }

    #endregion

    #region Exception Inheritance Tests

    [Fact]
    public void GtfsParsingException_ShouldInheritFromException()
    {
        // Arrange
        var exception = new GtfsParsingException("Test");

        // Act & Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void GtfsParsingException_CanBeCaughtAsException()
    {
        // Arrange & Act
        Action act = () => throw new GtfsParsingException("Test error");

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("Test error");
    }

    [Fact]
    public void GtfsParsingException_CanBeThrown_AndPreservesStackTrace()
    {
        // Arrange & Act
        Action act = () =>
        {
            try
            {
                throw new FormatException("Original error");
            }
            catch (Exception ex)
            {
                throw new GtfsParsingException("Wrapped error", ex);
            }
        };

        // Assert
        var exception = act.Should().Throw<GtfsParsingException>().Which;
        exception.InnerException.Should().BeOfType<FormatException>();
        exception.InnerException!.Message.Should().Be("Original error");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldAccept()
    {
        // Arrange & Act
        var exception = new GtfsParsingException(string.Empty);

        // Assert
        exception.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyFileName_ShouldNotHaveFileContext()
    {
        // Arrange & Act
        var exception = new GtfsParsingException("Error", string.Empty);

        // Assert
        exception.FileName.Should().BeEmpty();
        exception.HasFileContext.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void Constructor_WithVariousLineNumbers_ShouldFormatCorrectly(int lineNumber)
    {
        // Arrange & Act
        var exception = new GtfsParsingException("Error", "file.txt", lineNumber);

        // Assert
        exception.LineNumber.Should().Be(lineNumber);
        exception.Message.Should().Contain($"line {lineNumber}");
    }

    [Fact]
    public void MissingFiles_ShouldPreserveOrder()
    {
        // Arrange
        string[] files = ["calendar.txt", "stop_times.txt", "stops.txt", "routes.txt", "trips.txt"];

        // Act
        var exception = new GtfsParsingException(files);

        // Assert
        exception.MissingFiles.Should().BeEquivalentTo(files, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Constructor_WithAllGtfsFileTypes_ShouldListAllInMessage()
    {
        // Arrange
        string[] allRequiredFiles =
        [
            "stops.txt",
            "routes.txt",
            "trips.txt",
            "stop_times.txt",
            "calendar.txt"
        ];

        // Act
        var exception = new GtfsParsingException(allRequiredFiles);

        // Assert
        foreach (var file in allRequiredFiles)
        {
            exception.Message.Should().Contain(file);
        }
    }

    #endregion
}
