using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class StopTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var id = StopId.From("S1");
        var name = "Central Station";

        // Act
        var stop = new Stop(id, name);

        // Assert
        stop.Id.Should().Be(id);
        stop.Name.Should().Be(name);
        stop.Location.Should().BeNull();
        stop.PlatformCode.Should().BeNull();
        stop.WheelchairAccessible.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var id = StopId.From("S1");
        var name = "Central Station";
        var location = Coordinates.From(52.52, 13.405);

        // Act
        var stop = new Stop(id, name, location, "1A", true);

        // Assert
        stop.Location.Should().Be(location);
        stop.PlatformCode.Should().Be("1A");
        stop.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = StopId.From("S1");

        // Act
        var act = () => new Stop(id, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var id = StopId.From("S1");

        // Act
        var act = () => new Stop(id, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var id = StopId.From("S1");

        // Act
        var act = () => new Stop(id, "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithIdAndName_ShouldCreateSimpleStop()
    {
        // Act
        var stop = Stop.Create("S1", "Test Stop");

        // Assert
        stop.Id.Should().Be(StopId.From("S1"));
        stop.Name.Should().Be("Test Stop");
    }

    [Fact]
    public void Create_WithCoordinates_ShouldCreateStopWithLocation()
    {
        // Act
        var stop = Stop.Create("S1", "Test Stop", 52.52, 13.405);

        // Assert
        stop.Location.Should().NotBeNull();
        stop.Location!.Value.Latitude.Should().BeApproximately(52.52, 0.001);
        stop.Location!.Value.Longitude.Should().BeApproximately(13.405, 0.001);
    }

    [Fact]
    public void DistanceToKm_WithBothStopsHavingCoordinates_ShouldReturnDistance()
    {
        // Arrange - Approximately 1km apart
        var stopA = Stop.Create("A", "Stop A", 52.5200, 13.4050);
        var stopB = Stop.Create("B", "Stop B", 52.5290, 13.4050);

        // Act
        var distance = stopA.DistanceToKm(stopB);

        // Assert
        distance.Should().NotBeNull();
        distance.Should().BeApproximately(1.0, 0.1);  // ~1km with tolerance
    }

    [Fact]
    public void DistanceToKm_WithMissingCoordinates_ShouldReturnNull()
    {
        // Arrange
        var stopA = Stop.Create("A", "Stop A", 52.5200, 13.4050);
        var stopB = Stop.Create("B", "Stop B");  // No coordinates

        // Act
        var distance = stopA.DistanceToKm(stopB);

        // Assert
        distance.Should().BeNull();
    }

    [Fact]
    public void IsWithinKm_WhenWithinDistance_ShouldReturnTrue()
    {
        // Arrange
        var stopA = Stop.Create("A", "Stop A", 52.5200, 13.4050);
        var stopB = Stop.Create("B", "Stop B", 52.5210, 13.4050);  // ~100m north

        // Act
        var isWithin = stopA.IsWithinKm(stopB, 0.5);  // 500m

        // Assert
        isWithin.Should().BeTrue();
    }

    [Fact]
    public void IsWithinKm_WhenOutsideDistance_ShouldReturnFalse()
    {
        // Arrange
        var stopA = Stop.Create("A", "Stop A", 52.5200, 13.4050);
        var stopB = Stop.Create("B", "Stop B", 52.5400, 13.4050);  // ~2km north

        // Act
        var isWithin = stopA.IsWithinKm(stopB, 0.5);  // 500m

        // Assert
        isWithin.Should().BeFalse();
    }

    [Fact]
    public void Equality_StopsWithSameId_ShouldBeEqual()
    {
        // Arrange
        var stop1 = new Stop(StopId.From("S1"), "Name 1");
        var stop2 = new Stop(StopId.From("S1"), "Name 2");

        // Act & Assert
        stop1.Should().Be(stop2);
        (stop1 == stop2).Should().BeTrue();
        stop1.GetHashCode().Should().Be(stop2.GetHashCode());
    }

    [Fact]
    public void Equality_StopsWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var stop1 = new Stop(StopId.From("S1"), "Same Name");
        var stop2 = new Stop(StopId.From("S2"), "Same Name");

        // Act & Assert
        stop1.Should().NotBe(stop2);
        (stop1 != stop2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithNull_ShouldNotBeEqual()
    {
        // Arrange
        var stop = Stop.Create("S1", "Stop");

        // Act & Assert
        stop.Equals(null).Should().BeFalse();
        (stop == null).Should().BeFalse();
        (null == stop).Should().BeFalse();
    }

    [Fact]
    public void ToString_WithoutPlatform_ShouldShowNameAndId()
    {
        // Arrange
        var stop = Stop.Create("S1", "Central Station");

        // Act
        var result = stop.ToString();

        // Assert
        result.Should().Be("Central Station (S1)");
    }

    [Fact]
    public void ToString_WithPlatform_ShouldIncludePlatform()
    {
        // Arrange
        var stop = new Stop(StopId.From("S1"), "Central Station", platformCode: "2B");

        // Act
        var result = stop.ToString();

        // Assert
        result.Should().Contain("Platform 2B");
    }
}
