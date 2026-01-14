using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class TransportModeTests
{
    #region Enum Value Tests

    [Theory]
    [InlineData(TransportMode.Tram, 0)]
    [InlineData(TransportMode.Subway, 1)]
    [InlineData(TransportMode.Rail, 2)]
    [InlineData(TransportMode.Bus, 3)]
    [InlineData(TransportMode.Ferry, 4)]
    [InlineData(TransportMode.CableTram, 5)]
    [InlineData(TransportMode.AerialLift, 6)]
    [InlineData(TransportMode.Funicular, 7)]
    [InlineData(TransportMode.Trolleybus, 11)]
    [InlineData(TransportMode.Monorail, 12)]
    [InlineData(TransportMode.Walk, 100)]
    [InlineData(TransportMode.Unknown, 999)]
    public void TransportMode_HasCorrectGtfsValue(TransportMode mode, int expectedValue)
    {
        // Act & Assert
        ((int)mode).Should().Be(expectedValue);
    }

    #endregion

    #region GetDisplayName Tests

    [Theory]
    [InlineData(TransportMode.Tram, "Tram")]
    [InlineData(TransportMode.Subway, "Subway")]
    [InlineData(TransportMode.Rail, "Rail")]
    [InlineData(TransportMode.Bus, "Bus")]
    [InlineData(TransportMode.Ferry, "Ferry")]
    [InlineData(TransportMode.CableTram, "Cable Tram")]
    [InlineData(TransportMode.AerialLift, "Aerial Lift")]
    [InlineData(TransportMode.Funicular, "Funicular")]
    [InlineData(TransportMode.Trolleybus, "Trolleybus")]
    [InlineData(TransportMode.Monorail, "Monorail")]
    [InlineData(TransportMode.Walk, "Walk")]
    [InlineData(TransportMode.Unknown, "Unknown")]
    public void GetDisplayName_ReturnsCorrectEnglishName(TransportMode mode, string expectedName)
    {
        // Act
        var displayName = mode.GetDisplayName();

        // Assert
        displayName.Should().Be(expectedName);
    }

    [Fact]
    public void GetDisplayName_AllDefinedModes_ReturnsNonEmptyString()
    {
        // Act & Assert
        foreach (TransportMode mode in Enum.GetValues<TransportMode>())
        {
            var displayName = mode.GetDisplayName();
            displayName.Should().NotBeNullOrEmpty($"Mode {mode} should have a display name");
        }
    }

    #endregion

    #region GetDisplayNameDe Tests

    [Theory]
    [InlineData(TransportMode.Tram, "Straßenbahn")]
    [InlineData(TransportMode.Subway, "U-Bahn")]
    [InlineData(TransportMode.Rail, "Bahn")]
    [InlineData(TransportMode.Bus, "Bus")]
    [InlineData(TransportMode.Ferry, "Fähre")]
    [InlineData(TransportMode.CableTram, "Seilbahn")]
    [InlineData(TransportMode.AerialLift, "Luftseilbahn")]
    [InlineData(TransportMode.Funicular, "Standseilbahn")]
    [InlineData(TransportMode.Trolleybus, "Oberleitungsbus")]
    [InlineData(TransportMode.Monorail, "Einschienenbahn")]
    [InlineData(TransportMode.Walk, "Fußweg")]
    [InlineData(TransportMode.Unknown, "Unbekannt")]
    public void GetDisplayNameDe_ReturnsCorrectGermanName(TransportMode mode, string expectedName)
    {
        // Act
        var displayName = mode.GetDisplayNameDe();

        // Assert
        displayName.Should().Be(expectedName);
    }

    [Fact]
    public void GetDisplayNameDe_AllDefinedModes_ReturnsNonEmptyString()
    {
        // Act & Assert
        foreach (TransportMode mode in Enum.GetValues<TransportMode>())
        {
            var displayName = mode.GetDisplayNameDe();
            displayName.Should().NotBeNullOrEmpty($"Mode {mode} should have a German display name");
        }
    }

    #endregion

    #region GetAbbreviation Tests

    [Theory]
    [InlineData(TransportMode.Tram, 'T')]
    [InlineData(TransportMode.Subway, 'U')]
    [InlineData(TransportMode.Rail, 'R')]
    [InlineData(TransportMode.Bus, 'B')]
    [InlineData(TransportMode.Ferry, 'F')]
    [InlineData(TransportMode.CableTram, 'C')]
    [InlineData(TransportMode.AerialLift, 'A')]
    [InlineData(TransportMode.Funicular, 'K')]
    [InlineData(TransportMode.Trolleybus, 'O')]
    [InlineData(TransportMode.Monorail, 'M')]
    [InlineData(TransportMode.Walk, 'W')]
    [InlineData(TransportMode.Unknown, '?')]
    public void GetAbbreviation_ReturnsCorrectAbbreviation(TransportMode mode, char expectedAbbreviation)
    {
        // Act
        var abbreviation = mode.GetAbbreviation();

        // Assert
        abbreviation.Should().Be(expectedAbbreviation);
    }

    [Fact]
    public void GetAbbreviation_AllDefinedModes_ReturnsNonDefaultChar()
    {
        // Act & Assert
        foreach (TransportMode mode in Enum.GetValues<TransportMode>())
        {
            var abbreviation = mode.GetAbbreviation();
            abbreviation.Should().NotBe(default(char), $"Mode {mode} should have an abbreviation");
        }
    }

    #endregion

    #region IsRailBased Tests

    [Theory]
    [InlineData(TransportMode.Tram, true)]
    [InlineData(TransportMode.Subway, true)]
    [InlineData(TransportMode.Rail, true)]
    [InlineData(TransportMode.Monorail, true)]
    [InlineData(TransportMode.Funicular, true)]
    [InlineData(TransportMode.Bus, false)]
    [InlineData(TransportMode.Trolleybus, false)]
    [InlineData(TransportMode.Ferry, false)]
    [InlineData(TransportMode.Walk, false)]
    public void IsRailBased_ReturnsCorrectValue(TransportMode mode, bool expectedResult)
    {
        // Act
        var isRailBased = mode.IsRailBased();

        // Assert
        isRailBased.Should().Be(expectedResult);
    }

    #endregion

    #region IsRoadBased Tests

    [Theory]
    [InlineData(TransportMode.Bus, true)]
    [InlineData(TransportMode.Trolleybus, true)]
    [InlineData(TransportMode.Tram, false)]
    [InlineData(TransportMode.Subway, false)]
    [InlineData(TransportMode.Rail, false)]
    [InlineData(TransportMode.Ferry, false)]
    [InlineData(TransportMode.Walk, false)]
    public void IsRoadBased_ReturnsCorrectValue(TransportMode mode, bool expectedResult)
    {
        // Act
        var isRoadBased = mode.IsRoadBased();

        // Assert
        isRoadBased.Should().Be(expectedResult);
    }

    #endregion

    #region GetTypicalSpeedKmh Tests

    [Theory]
    [InlineData(TransportMode.Walk, 5)]
    [InlineData(TransportMode.Tram, 20)]
    [InlineData(TransportMode.Bus, 25)]
    [InlineData(TransportMode.Trolleybus, 25)]
    [InlineData(TransportMode.Subway, 35)]
    [InlineData(TransportMode.Rail, 60)]
    [InlineData(TransportMode.Ferry, 20)]
    [InlineData(TransportMode.CableTram, 15)]
    [InlineData(TransportMode.AerialLift, 25)]
    [InlineData(TransportMode.Funicular, 20)]
    [InlineData(TransportMode.Monorail, 40)]
    [InlineData(TransportMode.Unknown, 20)]
    public void GetTypicalSpeedKmh_ReturnsExpectedSpeed(TransportMode mode, int expectedSpeed)
    {
        // Act
        var speed = mode.GetTypicalSpeedKmh();

        // Assert
        speed.Should().Be(expectedSpeed);
    }

    [Fact]
    public void GetTypicalSpeedKmh_AllModes_ReturnPositiveValue()
    {
        // Act & Assert
        foreach (TransportMode mode in Enum.GetValues<TransportMode>())
        {
            mode.GetTypicalSpeedKmh().Should().BeGreaterThan(0);
        }
    }

    #endregion

    #region TryFromGtfsRouteType Tests

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
    public void TryFromGtfsRouteType_WithValidRouteType_ReturnsTrue(int routeType, TransportMode expectedMode)
    {
        // Act
        var result = TransportModeExtensions.TryFromGtfsRouteType(routeType, out var mode);

        // Assert
        result.Should().BeTrue();
        mode.Should().Be(expectedMode);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(13)]
    [InlineData(-1)]
    public void TryFromGtfsRouteType_WithInvalidRouteType_ReturnsFalse(int routeType)
    {
        // Act
        var result = TransportModeExtensions.TryFromGtfsRouteType(routeType, out var mode);

        // Assert
        result.Should().BeFalse();
        mode.Should().Be(TransportMode.Unknown);
    }

    #endregion
}
