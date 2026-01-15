using System.Globalization;
using Fahrplanauskunft.Infrastructure.Gtfs;

namespace Fahrplanauskunft.Tests.Unit.Gtfs;

/// <summary>
/// Unit tests for the <see cref="ImportProgress"/> class.
/// Tests verify property behavior, calculated properties, and edge cases.
/// </summary>
public class ImportProgressTests
{
    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var progress = new ImportProgress();

        // Assert
        progress.CurrentFile.Should().BeEmpty();
        progress.RecordsProcessed.Should().Be(0);
        progress.TotalRecords.Should().BeNull();
        progress.Status.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_ShouldSetAllProperties()
    {
        // Arrange
        const string file = "stops.txt";
        const int processed = 500;
        const int total = 1000;
        const string status = "Reading";

        // Act
        var progress = new ImportProgress(file, processed, total, status);

        // Assert
        progress.CurrentFile.Should().Be(file);
        progress.RecordsProcessed.Should().Be(processed);
        progress.TotalRecords.Should().Be(total);
        progress.Status.Should().Be(status);
    }

    [Fact]
    public void ParameterizedConstructor_WithoutTotalRecords_ShouldUseNull()
    {
        // Arrange & Act
        var progress = new ImportProgress("stops.txt", 100);

        // Assert
        progress.TotalRecords.Should().BeNull();
    }

    [Fact]
    public void ParameterizedConstructor_WithoutStatus_ShouldDefaultToParsing()
    {
        // Arrange & Act
        var progress = new ImportProgress("stops.txt", 100, 1000);

        // Assert
        progress.Status.Should().Be("Parsing");
    }

    #endregion

    #region PercentComplete Tests

    [Fact]
    public void PercentComplete_WhenTotalRecordsIsNull_ShouldReturnNull()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500);

        // Act & Assert
        progress.PercentComplete.Should().BeNull();
    }

    [Fact]
    public void PercentComplete_WhenTotalRecordsIsZero_ShouldReturnNull()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, 0);

        // Act & Assert
        progress.PercentComplete.Should().BeNull();
    }

    [Fact]
    public void PercentComplete_WhenTotalRecordsIsNegative_ShouldReturnNull()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, -1);

        // Act & Assert
        progress.PercentComplete.Should().BeNull();
    }

    [Fact]
    public void PercentComplete_WhenHalfwayComplete_ShouldReturn50()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500, 1000);

        // Act & Assert
        progress.PercentComplete.Should().Be(50.0);
    }

    [Fact]
    public void PercentComplete_WhenFullyComplete_ShouldReturn100()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 1000, 1000);

        // Act & Assert
        progress.PercentComplete.Should().Be(100.0);
    }

    [Fact]
    public void PercentComplete_WhenAtStart_ShouldReturn0()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, 1000);

        // Act & Assert
        progress.PercentComplete.Should().Be(0.0);
    }

    [Theory]
    [InlineData(1, 3, 33.333333333333336)] // 1/3
    [InlineData(2, 3, 66.666666666666671)] // 2/3
    [InlineData(1, 7, 14.285714285714286)] // 1/7
    public void PercentComplete_WithFractionalProgress_ShouldCalculateCorrectly(
        int processed, int total, double expectedPercent)
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", processed, total);

        // Act & Assert
        progress.PercentComplete.Should().BeApproximately(expectedPercent, 0.0001);
    }

    [Fact]
    public void PercentComplete_WithLargeNumbers_ShouldCalculateCorrectly()
    {
        // Arrange - Simulating 1M+ records as mentioned in spec
        var progress = new ImportProgress("stop_times.txt", 500000, 1000000);

        // Act & Assert
        progress.PercentComplete.Should().Be(50.0);
    }

    [Fact]
    public void PercentComplete_WhenRecordsExceedTotal_ShouldReturnOver100()
    {
        // Arrange - Edge case where processed > total (shouldn't normally happen)
        var progress = new ImportProgress("stops.txt", 1200, 1000);

        // Act & Assert
        progress.PercentComplete.Should().Be(120.0);
    }

    #endregion

    #region IsComplete Tests

    [Fact]
    public void IsComplete_WhenTotalRecordsIsNull_ShouldReturnFalse()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500);

        // Act & Assert
        progress.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WhenRecordsProcessedLessThanTotal_ShouldReturnFalse()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500, 1000);

        // Act & Assert
        progress.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WhenRecordsProcessedEqualsTotal_ShouldReturnTrue()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 1000, 1000);

        // Act & Assert
        progress.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_WhenRecordsProcessedExceedsTotal_ShouldReturnTrue()
    {
        // Arrange - Edge case
        var progress = new ImportProgress("stops.txt", 1100, 1000);

        // Act & Assert
        progress.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_WhenTotalRecordsIsZero_ShouldReturnTrue()
    {
        // Arrange - Zero total means already complete
        var progress = new ImportProgress("stops.txt", 0, 0);

        // Act & Assert
        progress.IsComplete.Should().BeTrue();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithPercentComplete_ShouldContainExpectedComponents()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500, 1000, "Parsing");

        // Act
        var result = progress.ToString();

        // Assert - Verify structure without being culture-specific for decimal separator
        result.Should().StartWith("[Parsing] stops.txt:");
        result.Should().EndWith("%");
        result.Should().Contain("50");
    }

    [Fact]
    public void ToString_WithoutTotalRecords_ShouldShowRecordCount()
    {
        // Arrange
        var progress = new ImportProgress("stop_times.txt", 12345, status: "Reading");

        // Act
        var result = progress.ToString();

        // Assert
        result.Should().Be("[Reading] stop_times.txt: 12345 records");
    }

    [Fact]
    public void ToString_WithZeroRecords_ShouldShowZeroRecords()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, status: "Starting");

        // Act
        var result = progress.ToString();

        // Assert
        result.Should().Be("[Starting] stops.txt: 0 records");
    }

    [Fact]
    public void ToString_WithFractionalPercent_ShouldContainPercentValue()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 333, 1000, "Parsing");

        // Act
        var result = progress.ToString();

        // Assert - Verify structure without being culture-specific for decimal separator
        result.Should().StartWith("[Parsing] stops.txt:");
        result.Should().EndWith("%");
        result.Should().Contain("33");
    }

    [Fact]
    public void ToString_WithCompletedStatus_ShouldContainExpectedComponents()
    {
        // Arrange
        var progress = new ImportProgress("routes.txt", 150, 150, "Completed");

        // Act
        var result = progress.ToString();

        // Assert - Verify structure without being culture-specific for decimal separator
        result.Should().StartWith("[Completed] routes.txt:");
        result.Should().EndWith("%");
        result.Should().Contain("100");
    }

    [Fact]
    public void ToString_ShouldFormatPercentageWithCurrentCulture()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 500, 1000, "Parsing");
        var expectedDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        // Act
        var result = progress.ToString();

        // Assert - The format should use the current culture's decimal separator
        result.Should().Contain($"50{expectedDecimalSeparator}0%");
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void CurrentFile_CanBeSetAfterConstruction()
    {
        // Arrange
        var progress = new ImportProgress();

        // Act
        progress.CurrentFile = "trips.txt";

        // Assert
        progress.CurrentFile.Should().Be("trips.txt");
    }

    [Fact]
    public void RecordsProcessed_CanBeIncrementedDuringParsing()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, 100);

        // Act - Simulate incremental progress
        progress.RecordsProcessed = 25;
        progress.RecordsProcessed.Should().Be(25);

        progress.RecordsProcessed = 50;
        progress.RecordsProcessed.Should().Be(50);

        progress.RecordsProcessed = 100;

        // Assert
        progress.RecordsProcessed.Should().Be(100);
        progress.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TotalRecords_CanBeSetAfterConstruction()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0);

        // Act
        progress.TotalRecords = 500;

        // Assert
        progress.TotalRecords.Should().Be(500);
        progress.PercentComplete.Should().Be(0.0);
    }

    [Fact]
    public void Status_CanBeUpdatedDuringParsing()
    {
        // Arrange
        var progress = new ImportProgress("stops.txt", 0, 100, "Starting");

        // Act & Assert
        progress.Status.Should().Be("Starting");

        progress.Status = "Parsing";
        progress.Status.Should().Be("Parsing");

        progress.Status = "Completed";
        progress.Status.Should().Be("Completed");
    }

    #endregion

    #region IProgress<ImportProgress> Usage Tests

    [Fact]
    public void ImportProgress_CanBeUsedWithIProgress()
    {
        // Arrange
        var reportedProgress = new List<ImportProgress>();
        var progressHandler = new Progress<ImportProgress>(p => reportedProgress.Add(p));

        // Act - Simulate how the parser would report progress
        var progress = new ImportProgress("stop_times.txt", 10000, 100000, "Parsing");
        ((IProgress<ImportProgress>)progressHandler).Report(progress);

        // Need to wait a bit for the progress to be reported (Progress<T> uses SynchronizationContext)
        Thread.Sleep(100);

        // Assert
        reportedProgress.Should().ContainSingle();
        reportedProgress[0].CurrentFile.Should().Be("stop_times.txt");
        reportedProgress[0].RecordsProcessed.Should().Be(10000);
        reportedProgress[0].TotalRecords.Should().Be(100000);
    }

    [Fact]
    public void ImportProgress_MultipleProgressReports_ShouldTrackCorrectValues()
    {
        // Arrange - Test that ImportProgress correctly represents progress at different stages
        // using a simpler, deterministic approach without relying on Progress<T> async behavior
        var progressStages = new List<ImportProgress>();

        // Act - Simulate multiple progress reports at 10k intervals (as mentioned in spec)
        for (int i = 10000; i <= 100000; i += 10000)
        {
            var progress = new ImportProgress("stop_times.txt", i, 100000, "Parsing");
            progressStages.Add(progress);
        }

        // Assert
        progressStages.Should().HaveCount(10);
        progressStages[0].RecordsProcessed.Should().Be(10000);
        progressStages[0].PercentComplete.Should().Be(10.0);
        progressStages[4].RecordsProcessed.Should().Be(50000);
        progressStages[4].PercentComplete.Should().Be(50.0);
        progressStages[9].RecordsProcessed.Should().Be(100000);
        progressStages[9].PercentComplete.Should().Be(100.0);
        progressStages[9].IsComplete.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ImportProgress_WithEmptyFileName_ShouldWork()
    {
        // Arrange & Act
        var progress = new ImportProgress(string.Empty, 100, 200);
        var expectedDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        // Assert
        progress.CurrentFile.Should().BeEmpty();
        progress.ToString().Should().Be($"[Parsing] : 50{expectedDecimalSeparator}0%");
    }

    [Fact]
    public void ImportProgress_WithNegativeRecordsProcessed_ShouldCalculateNegativePercent()
    {
        // Arrange - Edge case (shouldn't happen in practice)
        var progress = new ImportProgress("stops.txt", -100, 1000);

        // Act & Assert
        progress.PercentComplete.Should().Be(-10.0);
        progress.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void ImportProgress_AllGtfsFileNames_ShouldBeValidCurrentFile()
    {
        // Arrange - All GTFS files mentioned in the spec
        var gtfsFiles = new[]
        {
            "stops.txt",
            "routes.txt",
            "trips.txt",
            "stop_times.txt",
            "calendar.txt",
            "transfers.txt"
        };

        foreach (var file in gtfsFiles)
        {
            // Act
            var progress = new ImportProgress(file, 0, 100, "Parsing");

            // Assert
            progress.CurrentFile.Should().Be(file);
        }
    }

    [Fact]
    public void ImportProgress_WithVerySmallTotal_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new ImportProgress("transfers.txt", 1, 2);

        // Act & Assert
        progress.PercentComplete.Should().Be(50.0);
    }

    [Fact]
    public void ImportProgress_StatusTransitions_ShouldReflectParsingLifecycle()
    {
        // Arrange - Simulate a typical parsing lifecycle
        var progress = new ImportProgress();
        var expectedDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        // Starting phase
        progress.CurrentFile = "stops.txt";
        progress.Status = "Reading";
        progress.Status.Should().Be("Reading");

        // Parsing phase
        progress.Status = "Parsing";
        progress.TotalRecords = 1000;
        progress.RecordsProcessed = 500;
        progress.PercentComplete.Should().Be(50.0);

        // Completed phase
        progress.Status = "Completed";
        progress.RecordsProcessed = 1000;
        progress.IsComplete.Should().BeTrue();
        progress.ToString().Should().Be($"[Completed] stops.txt: 100{expectedDecimalSeparator}0%");
    }

    #endregion
}
