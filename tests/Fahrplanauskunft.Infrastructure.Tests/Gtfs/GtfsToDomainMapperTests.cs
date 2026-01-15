using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Gtfs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fahrplanauskunft.Infrastructure.Tests.Gtfs;

/// <summary>
/// Unit tests for the GtfsToDomainMapper class.
/// Tests verify the mapping of GTFS data transfer objects to domain entities.
/// </summary>
public class GtfsToDomainMapperTests
{
    private readonly ILogger<GtfsToDomainMapper> _logger;
    private readonly GtfsToDomainMapper _sut;

    public GtfsToDomainMapperTests()
    {
        _logger = Substitute.For<ILogger<GtfsToDomainMapper>>();
        _sut = new GtfsToDomainMapper(_logger);
    }

    #region MapStop Tests - Happy Path

    [Fact]
    public void MapStop_WithValidGtfsStop_ShouldReturnSuccessResult()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void MapStop_WithValidGtfsStop_ShouldMapStopIdCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(StopId.From("S1"));
    }

    [Fact]
    public void MapStop_WithValidGtfsStop_ShouldMapNameCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Central Station");
    }

    [Fact]
    public void MapStop_WithValidCoordinates_ShouldMapLocationCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().BeApproximately(52.5200, 0.0001);
        result.Value.Location!.Value.Longitude.Should().BeApproximately(13.4050, 0.0001);
    }

    [Fact]
    public void MapStop_WithPlatformCode_ShouldMapPlatformCodeCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050,
            PlatformCode = "1A"
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlatformCode.Should().Be("1A");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(null, false)]
    public void MapStop_WithWheelchairBoarding_ShouldMapAccessibilityCorrectly(int? wheelchairBoarding, bool expectedAccessible)
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050,
            WheelchairBoarding = wheelchairBoarding
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WheelchairAccessible.Should().Be(expectedAccessible);
    }

    #endregion

    #region MapStop Tests - Optional Fields

    [Fact]
    public void MapStop_WithoutCoordinates_ShouldHaveNullLocation()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Virtual Stop"
            // No coordinates
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().BeNull();
    }

    [Fact]
    public void MapStop_WithOnlyLatitude_ShouldHaveNullLocation()
    {
        // Arrange - Only latitude provided, no longitude
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Incomplete Stop",
            StopLat = 52.5200
            // No StopLon
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().BeNull();
    }

    [Fact]
    public void MapStop_WithOnlyLongitude_ShouldHaveNullLocation()
    {
        // Arrange - Only longitude provided, no latitude
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Incomplete Stop",
            StopLon = 13.4050
            // No StopLat
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().BeNull();
    }

    [Fact]
    public void MapStop_WithoutPlatformCode_ShouldHaveNullPlatformCode()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
            // No PlatformCode
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlatformCode.Should().BeNull();
    }

    [Fact]
    public void MapStop_WithEmptyPlatformCode_ShouldHaveEmptyPlatformCode()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050,
            PlatformCode = ""
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlatformCode.Should().BeEmpty();
    }

    #endregion

    #region MapStop Tests - Validation Errors

    [Fact]
    public void MapStop_WithNullGtfsStop_ShouldReturnFailure()
    {
        // Act
        var result = _sut.MapStop(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapStop_WithEmptyStopId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_id");
    }

    [Fact]
    public void MapStop_WithWhitespaceStopId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "   ",
            StopName = "Central Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_id");
    }

    [Fact]
    public void MapStop_WithNullStopName_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = null,
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_name");
    }

    [Fact]
    public void MapStop_WithEmptyStopName_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_name");
    }

    [Fact]
    public void MapStop_WithWhitespaceStopName_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "   ",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_name");
    }

    #endregion

    #region MapStop Tests - Invalid Coordinates

    [Theory]
    [InlineData(91.0, 13.4050)]   // Latitude too high
    [InlineData(-91.0, 13.4050)]  // Latitude too low
    [InlineData(52.5200, 181.0)]  // Longitude too high
    [InlineData(52.5200, -181.0)] // Longitude too low
    public void MapStop_WithInvalidCoordinates_ShouldReturnFailure(double lat, double lon)
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Invalid Location Stop",
            StopLat = lat,
            StopLon = lon
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        (result.Error.Contains("coordinates") ||
         result.Error.Contains("Latitude") ||
         result.Error.Contains("Longitude")).Should().BeTrue("error should mention coordinate-related issue");
    }

    [Fact]
    public void MapStop_WithLatitudeAtMaxBoundary_ShouldSucceed()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "North Pole",
            StopLat = 90.0,
            StopLon = 0.0
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().Be(90.0);
    }

    [Fact]
    public void MapStop_WithLatitudeAtMinBoundary_ShouldSucceed()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "South Pole",
            StopLat = -90.0,
            StopLon = 0.0
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().Be(-90.0);
    }

    [Fact]
    public void MapStop_WithLongitudeAtMaxBoundary_ShouldSucceed()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Date Line East",
            StopLat = 0.0,
            StopLon = 180.0
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Longitude.Should().Be(180.0);
    }

    [Fact]
    public void MapStop_WithLongitudeAtMinBoundary_ShouldSucceed()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Date Line West",
            StopLat = 0.0,
            StopLon = -180.0
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Longitude.Should().Be(-180.0);
    }

    #endregion

    #region MapStop Tests - Unicode and Special Characters

    [Fact]
    public void MapStop_WithGermanUmlauts_ShouldMapCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "München Hauptbahnhof",
            StopLat = 48.1402,
            StopLon = 11.5600
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("München Hauptbahnhof");
    }

    [Fact]
    public void MapStop_WithFrenchAccents_ShouldMapCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Gare de Lyon-Perrache",
            StopLat = 45.7485,
            StopLon = 4.8267
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Gare de Lyon-Perrache");
    }

    [Fact]
    public void MapStop_WithChineseCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "北京站",
            StopLat = 39.9042,
            StopLon = 116.4074
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("北京站");
    }

    [Fact]
    public void MapStop_WithSpecialCharactersInStopId_ShouldMapCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "de:09162:100_G",
            StopName = "Test Station",
            StopLat = 52.5200,
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(StopId.From("de:09162:100_G"));
    }

    #endregion

    #region MapStop Tests - Real-World Scenarios

    [Fact]
    public void MapStop_WithAllFieldsPopulated_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "de:09162:6",
            StopCode = "MUC",
            StopName = "München Hauptbahnhof",
            StopDesc = "Main railway station in Munich",
            StopLat = 48.1402,
            StopLon = 11.5600,
            ZoneId = "A",
            StopUrl = "https://www.muenchen.de/hbf",
            LocationType = 1,
            ParentStation = null,
            StopTimezone = "Europe/Berlin",
            WheelchairBoarding = 1,
            LevelId = "L1",
            PlatformCode = "1"
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stop = result.Value;
        stop.Id.Should().Be(StopId.From("de:09162:6"));
        stop.Name.Should().Be("München Hauptbahnhof");
        stop.Location.Should().NotBeNull();
        stop.Location!.Value.Latitude.Should().BeApproximately(48.1402, 0.0001);
        stop.Location!.Value.Longitude.Should().BeApproximately(11.5600, 0.0001);
        stop.PlatformCode.Should().Be("1");
        stop.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public void MapStop_WithMinimalRequiredFields_ShouldSucceed()
    {
        // Arrange - Only stop_id and stop_name are required for mapping
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Minimal Stop"
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(StopId.From("S1"));
        result.Value.Name.Should().Be("Minimal Stop");
        result.Value.Location.Should().BeNull();
        result.Value.PlatformCode.Should().BeNull();
        result.Value.WheelchairAccessible.Should().BeFalse();
    }

    [Fact]
    public void MapStop_WithHighPrecisionCoordinates_ShouldPreservePrecision()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Precise Location",
            StopLat = 52.5200389,
            StopLon = 13.4049539
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().BeApproximately(52.5200389, 0.0000001);
        result.Value.Location!.Value.Longitude.Should().BeApproximately(13.4049539, 0.0000001);
    }

    [Fact]
    public void MapStop_WithNegativeCoordinates_ShouldMapCorrectly()
    {
        // Arrange - Buenos Aires (Southern and Western hemispheres)
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Buenos Aires Retiro",
            StopLat = -34.6037,
            StopLon = -58.3816
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().Be(-34.6037);
        result.Value.Location!.Value.Longitude.Should().Be(-58.3816);
    }

    #endregion

    #region ParseGtfsTime Tests - Standard Times

    [Theory]
    [InlineData("00:00:00", 0)]       // Midnight
    [InlineData("01:30:00", 90)]      // 1:30 AM
    [InlineData("12:00:00", 720)]     // Noon
    [InlineData("23:59:00", 1439)]    // Last minute of day
    [InlineData("8:30:00", 510)]      // Single digit hour
    [InlineData("08:05:00", 485)]     // Leading zero
    public void ParseGtfsTime_WithStandardTime_ShouldReturnCorrectTotalMinutes(string timeString, int expectedMinutes)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalMinutes.Should().Be(expectedMinutes);
    }

    [Theory]
    [InlineData("00:00", 0)]       // Midnight without seconds
    [InlineData("12:30", 750)]     // Without seconds
    [InlineData("8:45", 525)]      // Single digit hour without seconds
    public void ParseGtfsTime_WithoutSeconds_ShouldReturnCorrectTotalMinutes(string timeString, int expectedMinutes)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalMinutes.Should().Be(expectedMinutes);
    }

    #endregion

    #region ParseGtfsTime Tests - Extended Hours

    [Fact]
    public void ParseGtfsTime_With25Hours30Minutes_ShouldReturn1530TotalMinutes()
    {
        // Arrange - "25:30:00" represents 1:30 AM next day in GTFS
        // 25 hours = 1500 minutes, plus 30 minutes = 1530 total
        var timeString = "25:30:00";

        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalMinutes.Should().Be(1530);
        result.Value.Hours.Should().Be(25);
        result.Value.Minutes.Should().Be(30);
        result.Value.IsExtended.Should().BeTrue();
    }

    [Theory]
    [InlineData("24:00:00", 1440)]   // Midnight next day
    [InlineData("24:30:00", 1470)]   // 12:30 AM next day
    [InlineData("25:00:00", 1500)]   // 1:00 AM next day
    [InlineData("26:15:00", 1575)]   // 2:15 AM next day
    [InlineData("27:45:00", 1665)]   // 3:45 AM next day
    [InlineData("28:00:00", 1680)]   // 4:00 AM next day
    [InlineData("29:30:00", 1770)]   // 5:30 AM next day
    [InlineData("30:00:00", 1800)]   // 6:00 AM next day
    [InlineData("47:59:00", 2879)]   // Maximum extended time (47:59)
    public void ParseGtfsTime_WithExtendedHours_ShouldReturnCorrectTotalMinutes(string timeString, int expectedMinutes)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalMinutes.Should().Be(expectedMinutes);
        result.Value.IsExtended.Should().BeTrue();
    }

    [Theory]
    [InlineData("24:00:00", 0, 0)]      // Midnight next day normalizes to 00:00
    [InlineData("25:30:00", 1, 30)]     // 25:30 normalizes to 01:30
    [InlineData("36:45:00", 12, 45)]    // 36:45 normalizes to 12:45
    [InlineData("47:59:00", 23, 59)]    // 47:59 normalizes to 23:59
    public void ParseGtfsTime_WithExtendedHours_NormalizedShouldReturnStandardTime(
        string timeString, int expectedNormalizedHours, int expectedNormalizedMinutes)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var normalized = result.Value.Normalized;
        normalized.Hours.Should().Be(expectedNormalizedHours);
        normalized.Minutes.Should().Be(expectedNormalizedMinutes);
        normalized.IsExtended.Should().BeFalse();
    }

    [Fact]
    public void ParseGtfsTime_WithExtendedHours_ShouldPreserveOriginalForDisplay()
    {
        // Arrange
        var timeString = "25:30:00";

        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ToString().Should().Be("25:30");
        result.Value.ToNormalizedString().Should().Be("01:30");
    }

    #endregion

    #region ParseGtfsTime Tests - Invalid Inputs

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseGtfsTime_WithNullOrEmptyString_ShouldReturnFailure(string? timeString)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("null or empty");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("12")]
    [InlineData("12:")]
    [InlineData(":30")]
    [InlineData("12:30:00:00")]
    public void ParseGtfsTime_WithInvalidFormat_ShouldReturnFailure(string timeString)
    {
        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ParseGtfsTime_WithNonNumericHours_ShouldReturnFailure()
    {
        // Act
        var result = _sut.ParseGtfsTime("XX:30:00");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("hours");
    }

    [Fact]
    public void ParseGtfsTime_WithNonNumericMinutes_ShouldReturnFailure()
    {
        // Act
        var result = _sut.ParseGtfsTime("12:XX:00");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("minutes");
    }

    [Fact]
    public void ParseGtfsTime_WithNonNumericSeconds_ShouldReturnFailure()
    {
        // Act
        var result = _sut.ParseGtfsTime("12:30:XX");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("seconds");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(48)]
    [InlineData(99)]
    public void ParseGtfsTime_WithHoursOutOfRange_ShouldReturnFailure(int hours)
    {
        // Arrange
        var timeString = $"{hours}:30:00";

        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Hours");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(99)]
    public void ParseGtfsTime_WithMinutesOutOfRange_ShouldReturnFailure(int minutes)
    {
        // Arrange
        var timeString = $"12:{minutes}:00";

        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Minutes");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(99)]
    public void ParseGtfsTime_WithSecondsOutOfRange_ShouldReturnFailure(int seconds)
    {
        // Arrange
        var timeString = $"12:30:{seconds}";

        // Act
        var result = _sut.ParseGtfsTime(timeString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Seconds");
    }

    #endregion

    #region MapStopTime Tests - Extended Hours

    [Fact]
    public void MapStopTime_WithExtendedHours_ShouldMapCorrectly()
    {
        // Arrange - Night bus arriving at 25:30 (1:30 AM next day)
        var stopEntity = CreateTestStop("S1", "Night Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "25:30:00",
            DepartureTime = "25:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.TotalMinutes.Should().Be(1530);  // 25*60 + 30 = 1530
        result.Value.Departure.TotalMinutes.Should().Be(1532); // 25*60 + 32 = 1532
        result.Value.Arrival.IsExtended.Should().BeTrue();
        result.Value.Departure.IsExtended.Should().BeTrue();
    }

    [Fact]
    public void MapStopTime_WithTripSpanningMidnight_ShouldMapAllStopsCorrectly()
    {
        // Arrange - Trip starts at 23:45 and ends at 01:15 (25:15) next day
        var stopEntity1 = CreateTestStop("S1", "Start Stop");
        var stopEntity2 = CreateTestStop("S2", "After Midnight Stop");

        var gtfsStopTime1 = new GtfsStopTime
        {
            TripId = "NightTrip",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "23:45:00",
            DepartureTime = "23:50:00"
        };

        var gtfsStopTime2 = new GtfsStopTime
        {
            TripId = "NightTrip",
            StopId = "S2",
            StopSequence = 2,
            ArrivalTime = "25:15:00",  // 1:15 AM next day
            DepartureTime = "25:15:00"
        };

        // Act
        var result1 = _sut.MapStopTime(gtfsStopTime1, stopEntity1);
        var result2 = _sut.MapStopTime(gtfsStopTime2, stopEntity2);

        // Assert - First stop: normal time
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Arrival.Hours.Should().Be(23);
        result1.Value.Arrival.Minutes.Should().Be(45);
        result1.Value.Arrival.IsExtended.Should().BeFalse();

        // Assert - Second stop: extended time
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Arrival.Hours.Should().Be(25);
        result2.Value.Arrival.Minutes.Should().Be(15);
        result2.Value.Arrival.IsExtended.Should().BeTrue();
        result2.Value.Arrival.TotalMinutes.Should().Be(1515); // 25*60 + 15
    }

    [Fact]
    public void MapStopTime_WithMidnightExactly_ShouldMapCorrectly()
    {
        // Arrange - Stop at exactly midnight (24:00:00)
        var stopEntity = CreateTestStop("S1", "Midnight Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "24:00:00",
            DepartureTime = "24:05:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.TotalMinutes.Should().Be(1440); // 24 * 60 = 1440
        result.Value.Arrival.Hours.Should().Be(24);
        result.Value.Arrival.Minutes.Should().Be(0);
        result.Value.Arrival.Normalized.Hours.Should().Be(0);
        result.Value.Arrival.Normalized.Minutes.Should().Be(0);
    }

    [Fact]
    public void MapStopTime_WithMaxExtendedTime_ShouldMapCorrectly()
    {
        // Arrange - Maximum extended time (47:59:00)
        var stopEntity = CreateTestStop("S1", "Late Night Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "47:59:00",
            DepartureTime = "47:59:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.TotalMinutes.Should().Be(2879); // 47*60 + 59 = 2879
        result.Value.Arrival.Hours.Should().Be(47);
        result.Value.Arrival.Minutes.Should().Be(59);
    }

    [Fact]
    public void MapStopTime_WithExtendedHoursAndDwellTime_ShouldCalculateDwellTimeCorrectly()
    {
        // Arrange - Night bus with 5 minute dwell time at 25:30
        var stopEntity = CreateTestStop("S1", "Night Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "25:30:00",
            DepartureTime = "25:35:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DwellTime.TotalMinutes.Should().Be(5);
    }

    [Fact]
    public void MapStopTime_WithInvalidExtendedHours_ShouldReturnFailure()
    {
        // Arrange - Hours exceeding maximum (48:00:00)
        var stopEntity = CreateTestStop("S1", "Invalid Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "48:00:00",
            DepartureTime = "48:00:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("arrival_time");
    }

    [Fact]
    public void MapStopTime_WithOnlyArrivalTimeExtended_ShouldUseArrivalForDeparture()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Night Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "25:30:00",
            DepartureTime = null  // No departure time provided
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.TotalMinutes.Should().Be(1530);
        result.Value.Departure.TotalMinutes.Should().Be(1530); // Same as arrival
    }

    [Fact]
    public void MapStopTime_WithOnlyDepartureTimeExtended_ShouldUseDepartureForArrival()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Night Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = null,  // No arrival time provided
            DepartureTime = "25:30:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.TotalMinutes.Should().Be(1530);  // Same as departure
        result.Value.Departure.TotalMinutes.Should().Be(1530);
    }

    #endregion

    #region MapStopTime Tests - Happy Path

    [Fact]
    public void MapStopTime_WithValidStopTime_ShouldReturnSuccessResult()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void MapStopTime_WithValidStopTime_ShouldMapTimesCorrectly()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:35:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Arrival.Hours.Should().Be(8);
        result.Value.Arrival.Minutes.Should().Be(30);
        result.Value.Departure.Hours.Should().Be(8);
        result.Value.Departure.Minutes.Should().Be(35);
        result.Value.DwellTime.TotalMinutes.Should().Be(5);
    }

    [Fact]
    public void MapStopTime_WithZeroBasedSequence_ShouldConvertToOneBased()
    {
        // Arrange - GTFS allows 0-based sequences, our domain uses 1-based
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 0,  // 0-based
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Sequence.Value.Should().Be(1);  // Converted to 1-based
    }

    [Theory]
    [InlineData(0, true)]   // Regularly scheduled
    [InlineData(null, true)] // Not specified = allowed
    [InlineData(1, false)]  // Not available
    [InlineData(2, false)]  // Phone agency
    [InlineData(3, false)]  // Coordinate with driver
    public void MapStopTime_WithPickupType_ShouldMapPickupAllowedCorrectly(int? pickupType, bool expectedAllowed)
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00",
            PickupType = pickupType
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PickupAllowed.Should().Be(expectedAllowed);
    }

    [Theory]
    [InlineData(0, true)]   // Regularly scheduled
    [InlineData(null, true)] // Not specified = allowed
    [InlineData(1, false)]  // Not available
    [InlineData(2, false)]  // Phone agency
    [InlineData(3, false)]  // Coordinate with driver
    public void MapStopTime_WithDropOffType_ShouldMapDropOffAllowedCorrectly(int? dropOffType, bool expectedAllowed)
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00",
            DropOffType = dropOffType
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DropOffAllowed.Should().Be(expectedAllowed);
    }

    [Fact]
    public void MapStopTime_WithStopReference_ShouldReferenceCorrectStop()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stop.Should().BeSameAs(stopEntity);
    }

    #endregion

    #region MapStopTime Tests - Validation Errors

    [Fact]
    public void MapStopTime_WithNullGtfsStopTime_ShouldReturnFailure()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");

        // Act
        var result = _sut.MapStopTime(null!, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapStopTime_WithNullStopEntity_ShouldReturnFailure()
    {
        // Arrange
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "12:00:00",
            DepartureTime = "12:05:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapStopTime_WithNoTimes_ShouldReturnFailure()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = null,
            DepartureTime = null
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("At least one of arrival_time or departure_time is required");
    }

    [Fact]
    public void MapStopTime_WithEmptyTripId_ShouldReturnFailure()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("trip_id");
    }

    [Fact]
    public void MapStopTime_WithEmptyStopId_ShouldReturnFailure()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "T1",
            StopId = "",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_id");
    }

    [Fact]
    public void MapStopTime_WithWhitespaceTripId_ShouldReturnFailure()
    {
        // Arrange
        var stopEntity = CreateTestStop("S1", "Test Stop");
        var gtfsStopTime = new GtfsStopTime
        {
            TripId = "   ",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "08:30:00",
            DepartureTime = "08:32:00"
        };

        // Act
        var result = _sut.MapStopTime(gtfsStopTime, stopEntity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("trip_id");
    }

    #endregion

    #region Helper Methods

    private static Stop CreateTestStop(string stopId, string name)
    {
        return new Stop(
            StopId.From(stopId),
            name,
            new Coordinates(52.5200, 13.4050),
            null,
            false);
    }

    #endregion

    #region MapTransportMode Tests - Standard GTFS Route Types

    [Theory]
    [InlineData(0, TransportMode.Tram)]
    [InlineData(1, TransportMode.Subway)]
    [InlineData(2, TransportMode.Rail)]
    [InlineData(3, TransportMode.Bus)]
    [InlineData(4, TransportMode.Ferry)]
    [InlineData(5, TransportMode.CableTram)]
    [InlineData(6, TransportMode.AerialLift)]
    [InlineData(7, TransportMode.Funicular)]
    [InlineData(11, TransportMode.Trolleybus)]
    [InlineData(12, TransportMode.Monorail)]
    public void MapTransportMode_WithStandardGtfsRouteType_ShouldReturnCorrectMode(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Fact]
    public void MapTransportMode_WithTram_ShouldReturnTram()
    {
        // Arrange - Route type 0 is Tram/Streetcar/Light Rail
        const int routeType = 0;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Tram);
        result.Value.GetDisplayName().Should().Be("Tram");
    }

    [Fact]
    public void MapTransportMode_WithSubway_ShouldReturnSubway()
    {
        // Arrange - Route type 1 is Subway/Metro
        const int routeType = 1;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Subway);
        result.Value.GetDisplayName().Should().Be("Subway");
    }

    [Fact]
    public void MapTransportMode_WithRail_ShouldReturnRail()
    {
        // Arrange - Route type 2 is Rail
        const int routeType = 2;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Rail);
        result.Value.IsRailBased().Should().BeTrue();
    }

    [Fact]
    public void MapTransportMode_WithBus_ShouldReturnBus()
    {
        // Arrange - Route type 3 is Bus
        const int routeType = 3;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Bus);
        result.Value.IsRoadBased().Should().BeTrue();
    }

    [Fact]
    public void MapTransportMode_WithFerry_ShouldReturnFerry()
    {
        // Arrange - Route type 4 is Ferry
        const int routeType = 4;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Ferry);
    }

    #endregion

    #region MapTransportMode Tests - Extended GTFS Route Types

    [Theory]
    [InlineData(101, TransportMode.Rail)]   // High Speed Rail Service
    [InlineData(102, TransportMode.Rail)]   // Long Distance Trains
    [InlineData(150, TransportMode.Rail)]   // Mid-range Railway Service
    [InlineData(199, TransportMode.Rail)]   // End of Railway Service range
    // Note: Route type 100 maps to Walk due to TransportMode.Walk = 100 in the enum
    public void MapTransportMode_WithExtendedRailwayService_ShouldReturnRail(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(200, TransportMode.Bus)]   // Coach Service
    [InlineData(201, TransportMode.Bus)]   // International Coach Service
    [InlineData(299, TransportMode.Bus)]   // End of Coach Service range
    public void MapTransportMode_WithExtendedCoachService_ShouldReturnBus(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(300, TransportMode.Rail)]   // Suburban Railway Service (S-Bahn)
    [InlineData(399, TransportMode.Rail)]   // End of Suburban Railway range
    public void MapTransportMode_WithExtendedSuburbanRailway_ShouldReturnRail(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(400, TransportMode.Subway)]   // Urban Railway Service
    [InlineData(401, TransportMode.Subway)]   // Metro Service
    [InlineData(499, TransportMode.Subway)]   // End of Urban Railway range
    public void MapTransportMode_WithExtendedUrbanRailway_ShouldReturnSubway(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(500, TransportMode.Subway)]   // Metro Service
    [InlineData(599, TransportMode.Subway)]   // End of Metro Service range
    public void MapTransportMode_WithExtendedMetroService_ShouldReturnSubway(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(600, TransportMode.Subway)]   // Underground Service
    [InlineData(699, TransportMode.Subway)]   // End of Underground Service range
    public void MapTransportMode_WithExtendedUndergroundService_ShouldReturnSubway(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(700, TransportMode.Tram)]   // Tram Service
    [InlineData(799, TransportMode.Tram)]   // End of Tram Service range
    public void MapTransportMode_WithExtendedTramService_ShouldReturnTram(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(800, TransportMode.Bus)]   // Bus Service
    [InlineData(801, TransportMode.Bus)]   // Regional Bus Service
    [InlineData(899, TransportMode.Bus)]   // End of Bus Service range
    public void MapTransportMode_WithExtendedBusService_ShouldReturnBus(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(900, TransportMode.Trolleybus)]   // Trolleybus Service
    [InlineData(950, TransportMode.Trolleybus)]   // Mid-range Trolleybus Service
    [InlineData(998, TransportMode.Trolleybus)]   // End of Trolleybus Service range (999 maps to Unknown due to enum value)
    public void MapTransportMode_WithExtendedTrolleybusService_ShouldReturnTrolleybus(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1000, TransportMode.Ferry)]   // Water Transport Service
    [InlineData(1099, TransportMode.Ferry)]   // End of Water Transport range
    public void MapTransportMode_WithExtendedWaterTransport_ShouldReturnFerry(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1100, TransportMode.Unknown)]   // Air Service (not supported)
    [InlineData(1199, TransportMode.Unknown)]   // End of Air Service range
    public void MapTransportMode_WithExtendedAirService_ShouldReturnUnknown(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1200, TransportMode.Ferry)]   // Ferry Service
    [InlineData(1299, TransportMode.Ferry)]   // End of Ferry Service range
    public void MapTransportMode_WithExtendedFerryService_ShouldReturnFerry(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1300, TransportMode.AerialLift)]   // Aerial Lift Service
    [InlineData(1399, TransportMode.AerialLift)]   // End of Aerial Lift Service range
    public void MapTransportMode_WithExtendedAerialLiftService_ShouldReturnAerialLift(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1400, TransportMode.Funicular)]   // Funicular Service
    [InlineData(1499, TransportMode.Funicular)]   // End of Funicular Service range
    public void MapTransportMode_WithExtendedFunicularService_ShouldReturnFunicular(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1500, TransportMode.Unknown)]   // Taxi Service (not supported)
    [InlineData(1599, TransportMode.Unknown)]   // End of Taxi Service range
    public void MapTransportMode_WithExtendedTaxiService_ShouldReturnUnknown(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1600, TransportMode.Unknown)]   // Miscellaneous Service
    [InlineData(1699, TransportMode.Unknown)]   // End of Miscellaneous Service range
    public void MapTransportMode_WithExtendedMiscellaneousService_ShouldReturnUnknown(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(1700, TransportMode.CableTram)]   // Cable Car Service
    [InlineData(1799, TransportMode.CableTram)]   // End of Cable Car Service range
    public void MapTransportMode_WithExtendedCableCarService_ShouldReturnCableTram(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedMode);
    }

    #endregion

    #region MapTransportMode Tests - Enum Value Edge Cases

    [Fact]
    public void MapTransportMode_WithWalkEnumValue_ShouldReturnWalk()
    {
        // Arrange - Route type 100 matches the TransportMode.Walk enum value
        // This takes precedence over the extended Railway Service range (100-199)
        const int routeType = 100;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Walk);
    }

    [Fact]
    public void MapTransportMode_WithUnknownEnumValue_ShouldReturnUnknown()
    {
        // Arrange - Route type 999 matches the TransportMode.Unknown enum value
        // This takes precedence over the extended Trolleybus Service range (900-999)
        const int routeType = 999;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Unknown);
    }

    #endregion

    #region MapTransportMode Tests - Invalid and Unknown Route Types

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void MapTransportMode_WithNegativeRouteType_ShouldReturnFailure(int routeType)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown GTFS route_type");
    }

    [Theory]
    [InlineData(8)]    // Not defined in standard GTFS
    [InlineData(9)]    // Not defined in standard GTFS
    [InlineData(10)]   // Not defined in standard GTFS
    [InlineData(13)]   // Beyond defined standard types
    [InlineData(50)]   // Not in any range
    [InlineData(99)]   // Just below extended ranges
    public void MapTransportMode_WithUndefinedStandardType_ShouldReturnFailure(int routeType)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown GTFS route_type");
        result.Error.Should().Contain(routeType.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(1800)]    // Beyond extended range
    [InlineData(2000)]    // Way beyond extended range
    [InlineData(9999)]    // Very large number
    public void MapTransportMode_WithRouteTypeBeyondExtendedRange_ShouldReturnFailure(int routeType)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown GTFS route_type");
    }

    #endregion

    #region MapTransportMode Tests - Real-World Scenarios

    [Fact]
    public void MapTransportMode_WithGermanSBahn_ShouldReturnRail()
    {
        // Arrange - German S-Bahn uses extended route_type 300
        const int routeType = 300;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Rail);
        result.Value.GetDisplayNameDe().Should().Be("Bahn");
    }

    [Fact]
    public void MapTransportMode_WithBerlinUBahn_ShouldReturnSubway()
    {
        // Arrange - Berlin U-Bahn uses extended route_type 400
        const int routeType = 400;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Subway);
        result.Value.GetDisplayNameDe().Should().Be("U-Bahn");
    }

    [Fact]
    public void MapTransportMode_WithMunichTram_ShouldReturnTram()
    {
        // Arrange - Munich tram uses extended route_type 700
        const int routeType = 700;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Tram);
        result.Value.GetDisplayNameDe().Should().Be("Straßenbahn");
    }

    [Fact]
    public void MapTransportMode_WithRegionalBus_ShouldReturnBus()
    {
        // Arrange - Regional bus uses extended route_type 801
        const int routeType = 801;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Bus);
        result.Value.GetAbbreviation().Should().Be('B');
    }

    [Fact]
    public void MapTransportMode_WithHighSpeedRail_ShouldReturnRail()
    {
        // Arrange - ICE/TGV High Speed Rail uses extended route_type 101
        const int routeType = 101;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Rail);
        result.Value.GetTypicalSpeedKmh().Should().Be(60); // Default rail speed
    }

    [Fact]
    public void MapTransportMode_WithSwissGondola_ShouldReturnAerialLift()
    {
        // Arrange - Swiss gondola uses extended route_type 1300
        const int routeType = 1300;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.AerialLift);
        result.Value.GetDisplayName().Should().Be("Aerial Lift");
    }

    [Fact]
    public void MapTransportMode_WithStandardFunicular_ShouldReturnFunicular()
    {
        // Arrange - Standard GTFS funicular route type 7
        const int routeType = 7;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.Funicular);
        result.Value.IsRailBased().Should().BeTrue();
    }

    #endregion

    #region MapTransportMode Tests - Boundary Values

    [Theory]
    [InlineData(99, false)]    // Just before Railway Service range
    [InlineData(100, true)]    // Walk enum value (100) - matches as Walk, not Rail
    [InlineData(101, true)]    // Railway Service range
    [InlineData(199, true)]    // End of Railway Service range
    [InlineData(200, true)]    // Start of Coach Service range
    public void MapTransportMode_AtExtendedRangeBoundaries_ShouldBehaveCorrectly(int routeType, bool shouldSucceed)
    {
        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().Be(shouldSucceed);
    }

    [Fact]
    public void MapTransportMode_WithLastValidExtendedType_ShouldReturnCableTram()
    {
        // Arrange - Route type 1799 is the last valid extended type (Cable Car)
        const int routeType = 1799;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TransportMode.CableTram);
    }

    [Fact]
    public void MapTransportMode_WithFirstInvalidAfterExtended_ShouldReturnFailure()
    {
        // Arrange - Route type 1800 is just beyond the valid extended range
        const int routeType = 1800;

        // Act
        var result = _sut.MapTransportMode(routeType);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region MapStop Tests - Error Message Quality

    [Fact]
    public void MapStop_WithEmptyStopId_ErrorShouldMentionStopId()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "",
            StopName = "Test Stop"
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("stop_id");
    }

    [Fact]
    public void MapStop_WithInvalidCoordinates_ErrorShouldBeDescriptive()
    {
        // Arrange
        var gtfsStop = new GtfsStop
        {
            StopId = "S1",
            StopName = "Invalid Stop",
            StopLat = 100.0,  // Invalid latitude
            StopLon = 13.4050
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("S1"); // Should mention the stop ID in the error
    }

    #endregion

    #region MapRoute Tests - Happy Path

    [Fact]
    public void MapRoute_WithValidGtfsRoute_ShouldReturnSuccessResult()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteType = 1 // Subway
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void MapRoute_WithValidGtfsRoute_ShouldMapRouteIdCorrectly()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(RouteId.From("R1"));
    }

    [Fact]
    public void MapRoute_WithValidGtfsRoute_ShouldMapShortNameCorrectly()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("U1");
    }

    [Fact]
    public void MapRoute_WithLongNameOnly_ShouldUseLongNameAsShortName()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteLongName = "Downtown Express",
            RouteType = 3 // Bus
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("Downtown Express");
        result.Value.LongName.Should().BeNull(); // Long name not set when same as short name
    }

    [Fact]
    public void MapRoute_WithBothNames_ShouldSetBothCorrectly()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteLongName = "University Line",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("U1");
        result.Value.LongName.Should().Be("University Line");
    }

    [Fact]
    public void MapRoute_WithRouteColors_ShouldMapColorsCorrectly()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteType = 1,
            RouteColor = "FF0000",
            RouteTextColor = "FFFFFF"
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Color.Should().Be("FF0000");
        result.Value.TextColor.Should().Be("FFFFFF");
    }

    [Theory]
    [InlineData(0, TransportMode.Tram)]
    [InlineData(1, TransportMode.Subway)]
    [InlineData(2, TransportMode.Rail)]
    [InlineData(3, TransportMode.Bus)]
    public void MapRoute_WithVariousRouteTypes_ShouldMapTransportModeCorrectly(int routeType, TransportMode expectedMode)
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "Line 1",
            RouteType = routeType
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Mode.Should().Be(expectedMode);
    }

    #endregion

    #region MapRoute Tests - Validation Errors

    [Fact]
    public void MapRoute_WithNullGtfsRoute_ShouldReturnFailure()
    {
        // Act
        var result = _sut.MapRoute(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapRoute_WithEmptyRouteId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "",
            RouteShortName = "U1",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_id");
    }

    [Fact]
    public void MapRoute_WithNoNames_ShouldReturnFailure()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = null,
            RouteLongName = null,
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_short_name");
        result.Error.Should().Contain("route_long_name");
    }

    [Fact]
    public void MapRoute_WithUnknownRouteType_ShouldDefaultToUnknown()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "X1",
            RouteType = 9999 // Unknown type
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Mode.Should().Be(TransportMode.Unknown);
    }

    [Fact]
    public void MapRoute_WithWhitespaceRouteId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "   ",
            RouteShortName = "U1",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_id");
    }

    [Fact]
    public void MapRoute_WithWhitespaceNames_ShouldReturnFailure()
    {
        // Arrange
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "   ",
            RouteLongName = "   ",
            RouteType = 1
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_short_name");
        result.Error.Should().Contain("route_long_name");
    }

    [Fact]
    public void MapRoute_WithEmptyColors_ShouldMapCorrectly()
    {
        // Arrange - Empty strings for colors should work
        var gtfsRoute = new GtfsRoute
        {
            RouteId = "R1",
            RouteShortName = "U1",
            RouteType = 1,
            RouteColor = "",
            RouteTextColor = ""
        };

        // Act
        var result = _sut.MapRoute(gtfsRoute);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Color.Should().Be("");
        result.Value.TextColor.Should().Be("");
    }

    #endregion

    #region MapTrip Tests - Happy Path

    [Fact]
    public void MapTrip_WithValidGtfsTrip_ShouldReturnSuccessResult()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void MapTrip_WithValidGtfsTrip_ShouldMapTripIdCorrectly()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(TripId.From("T1"));
    }

    [Fact]
    public void MapTrip_WithHeadsign_ShouldMapHeadsignCorrectly()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "WEEKDAY",
            TripHeadsign = "Central Station"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Headsign.Should().Be("Central Station");
    }

    [Fact]
    public void MapTrip_WithShortName_ShouldMapShortNameCorrectly()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "WEEKDAY",
            TripShortName = "Express 42"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortName.Should().Be("Express 42");
    }

    [Fact]
    public void MapTrip_WithMinimalFields_ShouldSucceed()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(TripId.From("T1"));
        result.Value.Headsign.Should().BeNull();
        result.Value.ShortName.Should().BeNull();
    }

    #endregion

    #region MapTrip Tests - Validation Errors

    [Fact]
    public void MapTrip_WithNullGtfsTrip_ShouldReturnFailure()
    {
        // Act
        var result = _sut.MapTrip(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapTrip_WithEmptyTripId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "",
            RouteId = "R1",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("trip_id");
    }

    [Fact]
    public void MapTrip_WithEmptyRouteId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_id");
    }

    [Fact]
    public void MapTrip_WithEmptyServiceId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = ""
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("service_id");
    }

    [Fact]
    public void MapTrip_WithWhitespaceTripId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "   ",
            RouteId = "R1",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("trip_id");
    }

    [Fact]
    public void MapTrip_WithWhitespaceRouteId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "   ",
            ServiceId = "WEEKDAY"
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("route_id");
    }

    [Fact]
    public void MapTrip_WithWhitespaceServiceId_ShouldReturnFailure()
    {
        // Arrange
        var gtfsTrip = new GtfsTrip
        {
            TripId = "T1",
            RouteId = "R1",
            ServiceId = "   "
        };

        // Act
        var result = _sut.MapTrip(gtfsTrip);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("service_id");
    }

    #endregion

    #region MapTransfer Tests - Happy Path

    [Fact]
    public void MapTransfer_WithValidTransfer_ShouldReturnSuccessResult()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0, // Recommended transfer
            MinTransferTime = 120 // 2 minutes
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void MapTransfer_WithMinTransferTime_ShouldMapDurationCorrectly()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 2, // Requires minimum time
            MinTransferTime = 180 // 3 minutes
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(3);
    }

    [Fact]
    public void MapTransfer_WithSecondsRoundUp_ShouldRoundUpToNextMinute()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 2,
            MinTransferTime = 121 // 2 minutes and 1 second -> should round up to 3
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(3);
    }

    [Fact]
    public void MapTransfer_WithDefaultTime_ShouldUseDefaultDuration()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0 // Recommended transfer, no time specified
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(2); // Default 2 minutes
    }

    [Fact]
    public void MapTransfer_WithBothStopsAccessible_ShouldBeWheelchairAccessible()
    {
        // Arrange
        var fromStop = new Stop(
            StopId.From("S1"),
            "Origin",
            new Coordinates(52.5200, 13.4050),
            null,
            true); // Wheelchair accessible
        var toStop = new Stop(
            StopId.From("S2"),
            "Destination",
            new Coordinates(52.5300, 13.4150),
            null,
            true); // Wheelchair accessible
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0,
            MinTransferTime = 120
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public void MapTransfer_WithOneStopNotAccessible_ShouldNotBeWheelchairAccessible()
    {
        // Arrange
        var fromStop = new Stop(
            StopId.From("S1"),
            "Origin",
            new Coordinates(52.5200, 13.4050),
            null,
            true); // Wheelchair accessible
        var toStop = new Stop(
            StopId.From("S2"),
            "Destination",
            new Coordinates(52.5300, 13.4150),
            null,
            false); // NOT wheelchair accessible
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0,
            MinTransferTime = 120
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WheelchairAccessible.Should().BeFalse();
    }

    #endregion

    #region MapTransfer Tests - Validation Errors

    [Fact]
    public void MapTransfer_WithNullGtfsTransfer_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");

        // Act
        var result = _sut.MapTransfer(null!, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapTransfer_WithNullFromStop_ShouldReturnFailure()
    {
        // Arrange
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, null!, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapTransfer_WithNullToStop_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void MapTransfer_WithEmptyFromStopId_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "",
            ToStopId = "S2",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("from_stop_id");
    }

    [Fact]
    public void MapTransfer_WithEmptyToStopId_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("to_stop_id");
    }

    [Fact]
    public void MapTransfer_WithSameFromAndToStop_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Same Stop");
        var toStop = CreateTestStop("S1", "Same Stop");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S1", // Same as from
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("same");
    }

    #endregion

    #region MapTransfer Tests - Transfer Types

    [Fact]
    public void MapTransfer_WithTransferType3_ShouldReturnFailure()
    {
        // Arrange - Transfer type 3 means transfers are not possible
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 3 // Not possible
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not possible");
    }

    [Fact]
    public void MapTransfer_WithTransferType4_ShouldReturnFailure()
    {
        // Arrange - Transfer type 4 is in-seat transfer (not walking)
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 4 // In-seat transfer
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("In-seat");
    }

    [Fact]
    public void MapTransfer_WithTransferType5_ShouldReturnFailure()
    {
        // Arrange - Transfer type 5 is in-seat transfer not allowed
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 5 // In-seat transfer not allowed
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("In-seat");
    }

    [Fact]
    public void MapTransfer_WithTransferType2AndNoMinTime_ShouldReturnFailure()
    {
        // Arrange - Transfer type 2 requires min_transfer_time
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 2, // Requires minimum time
            MinTransferTime = null // But no time specified
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("min_transfer_time");
    }

    [Theory]
    [InlineData(0)] // Recommended
    [InlineData(1)] // Timed
    public void MapTransfer_WithRecommendedOrTimedTransfer_ShouldSucceed(int transferType)
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = transferType,
            MinTransferTime = 120
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapTransfer_WithExactly60Seconds_ShouldMapToOneMinute()
    {
        // Arrange - 60 seconds should map to exactly 1 minute (no rounding needed)
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 2,
            MinTransferTime = 60
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(1);
    }

    [Fact]
    public void MapTransfer_WithWhitespaceFromStopId_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "   ",
            ToStopId = "S2",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("from_stop_id");
    }

    [Fact]
    public void MapTransfer_WithWhitespaceToStopId_ShouldReturnFailure()
    {
        // Arrange
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "   ",
            TransferType = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("to_stop_id");
    }

    [Fact]
    public void MapTransfer_WithUnknownTransferType_ShouldUseDefaultDuration()
    {
        // Arrange - Transfer types 6+ are not defined in GTFS spec, should default to 2 minutes
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 6 // Unknown type
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(2); // Default duration
    }

    [Fact]
    public void MapTransfer_WithZeroMinTransferTime_ShouldUseDefault()
    {
        // Arrange - 0 seconds is treated as "no time specified"
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0,
            MinTransferTime = 0
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(2); // Default when 0
    }

    [Fact]
    public void MapTransfer_WithNegativeMinTransferTime_ShouldUseDefault()
    {
        // Arrange - Negative values should use default
        var fromStop = CreateTestStop("S1", "Origin");
        var toStop = CreateTestStop("S2", "Destination");
        var gtfsTransfer = new GtfsTransfer
        {
            FromStopId = "S1",
            ToStopId = "S2",
            TransferType = 0,
            MinTransferTime = -60
        };

        // Act
        var result = _sut.MapTransfer(gtfsTransfer, fromStop, toStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.TotalMinutes.Should().Be(2); // Default when negative
    }

    #endregion

    #region Integration-like Scenario Tests

    [Fact]
    public void MappingScenario_CompleteStopWithAllFields_ShouldMapAllCorrectly()
    {
        // Arrange - A complete real-world-like stop
        var gtfsStop = new GtfsStop
        {
            StopId = "de:09162:2",
            StopName = "München Marienplatz",
            StopLat = 48.1372,
            StopLon = 11.5754,
            PlatformCode = "Gleis 1",
            WheelchairBoarding = 1
        };

        // Act
        var result = _sut.MapStop(gtfsStop);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stop = result.Value;
        stop.Id.Value.Should().Be("de:09162:2");
        stop.Name.Should().Be("München Marienplatz");
        stop.Location.Should().NotBeNull();
        stop.Location!.Value.Latitude.Should().BeApproximately(48.1372, 0.0001);
        stop.Location!.Value.Longitude.Should().BeApproximately(11.5754, 0.0001);
        stop.PlatformCode.Should().Be("Gleis 1");
        stop.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public void MappingScenario_NightBusTrip_ShouldHandleExtendedHoursCorrectly()
    {
        // Arrange - Night bus starting before midnight and ending after midnight
        var gtfsTrip = new GtfsTrip
        {
            TripId = "NightBus42_2300",
            RouteId = "N42",
            ServiceId = "WEEKNIGHT",
            TripHeadsign = "Airport via Downtown"
        };

        var stop1 = CreateTestStop("S1", "Downtown Station");
        var stop2 = CreateTestStop("S2", "Airport Terminal");

        var stopTime1 = new GtfsStopTime
        {
            TripId = "NightBus42_2300",
            StopId = "S1",
            StopSequence = 1,
            ArrivalTime = "23:30:00",
            DepartureTime = "23:35:00"
        };

        var stopTime2 = new GtfsStopTime
        {
            TripId = "NightBus42_2300",
            StopId = "S2",
            StopSequence = 2,
            ArrivalTime = "25:15:00",  // 1:15 AM next day
            DepartureTime = "25:15:00"
        };

        // Act
        var tripResult = _sut.MapTrip(gtfsTrip);
        var st1Result = _sut.MapStopTime(stopTime1, stop1);
        var st2Result = _sut.MapStopTime(stopTime2, stop2);

        // Assert
        tripResult.IsSuccess.Should().BeTrue();
        st1Result.IsSuccess.Should().BeTrue();
        st2Result.IsSuccess.Should().BeTrue();

        st1Result.Value.Arrival.IsExtended.Should().BeFalse();
        st2Result.Value.Arrival.IsExtended.Should().BeTrue();
        st2Result.Value.Arrival.TotalMinutes.Should().Be(1515);  // 25*60 + 15
    }

    [Fact]
    public void MappingScenario_MultiModalRoute_ShouldMapCorrectTransportModes()
    {
        // Arrange - Different route types for multi-modal network
        var busRoute = new GtfsRoute { RouteId = "B1", RouteShortName = "100", RouteType = 3 };
        var subwayRoute = new GtfsRoute { RouteId = "U1", RouteShortName = "U1", RouteType = 1 };
        var tramRoute = new GtfsRoute { RouteId = "T1", RouteShortName = "19", RouteType = 0 };
        var sbahn = new GtfsRoute { RouteId = "S1", RouteShortName = "S1", RouteType = 300 };  // Extended type

        // Act
        var busResult = _sut.MapRoute(busRoute);
        var subwayResult = _sut.MapRoute(subwayRoute);
        var tramResult = _sut.MapRoute(tramRoute);
        var sbahnResult = _sut.MapRoute(sbahn);

        // Assert
        busResult.Value.Mode.Should().Be(TransportMode.Bus);
        subwayResult.Value.Mode.Should().Be(TransportMode.Subway);
        tramResult.Value.Mode.Should().Be(TransportMode.Tram);
        sbahnResult.Value.Mode.Should().Be(TransportMode.Rail);  // S-Bahn maps to Rail
    }

    #endregion
}
