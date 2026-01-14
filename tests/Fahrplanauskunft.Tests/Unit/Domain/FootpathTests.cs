using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class FootpathTests
{
    private static Stop CreateStop(string id, string name, double? lat = null, double? lon = null)
    {
        if (lat.HasValue && lon.HasValue)
            return Stop.Create(id, name, lat.Value, lon.Value);
        return Stop.Create(id, name);
    }

    [Fact]
    public void Constructor_WithDifferentStops_ShouldSucceed()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");
        var duration = Duration.FromMinutes(5);

        // Act
        var footpath = new Footpath(stopA, stopB, duration);

        // Assert
        footpath.From.Should().Be(stopA);
        footpath.To.Should().Be(stopB);
        footpath.Duration.Should().Be(duration);
    }

    [Fact]
    public void Constructor_WithSameStop_ShouldThrowSelfLoopException()
    {
        // Arrange
        var stop = CreateStop("A", "Stop A");
        var duration = Duration.FromMinutes(0);

        // Act
        var act = () => new Footpath(stop, stop, duration);

        // Assert
        act.Should().Throw<SelfLoopException>()
            .WithMessage("*same origin and destination*");
    }

    [Fact]
    public void Constructor_WithStopsHavingSameId_ShouldThrowSelfLoopException()
    {
        // Arrange - Two stops with same ID are considered equal
        var stopA = CreateStop("SAME", "Stop A Name");
        var stopB = CreateStop("SAME", "Stop B Name");  // Same ID but different name
        var duration = Duration.FromMinutes(5);

        // Act
        var act = () => new Footpath(stopA, stopB, duration);

        // Assert
        act.Should().Throw<SelfLoopException>();
    }

    [Fact]
    public void Constructor_WithNullFrom_ShouldThrowArgumentNullException()
    {
        // Arrange
        var stopB = CreateStop("B", "Stop B");
        var duration = Duration.FromMinutes(5);

        // Act
        var act = () => new Footpath(null!, stopB, duration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullTo_ShouldThrowArgumentNullException()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var duration = Duration.FromMinutes(5);

        // Act
        var act = () => new Footpath(stopA, null!, duration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldCreateFootpathWithMinutes()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        // Act
        var footpath = Footpath.Create(stopA, stopB, 10);

        // Assert
        footpath.Duration.TotalMinutes.Should().Be(10);
    }

    [Fact]
    public void CreateFromDistance_ShouldEstimateWalkingTime()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");
        var distanceMeters = 500;  // 500m

        // Act
        var footpath = Footpath.CreateFromDistance(stopA, stopB, distanceMeters);

        // Assert
        // 500m at 5 km/h (83.33 m/min) = ~6 minutes
        footpath.Duration.TotalMinutes.Should().Be(6);
        footpath.DistanceMeters.Should().Be(500);
    }

    [Fact]
    public void CreateBidirectional_ShouldCreateBothDirections()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");
        var duration = Duration.FromMinutes(5);

        // Act
        var (forward, backward) = Footpath.CreateBidirectional(stopA, stopB, duration);

        // Assert
        forward.From.Should().Be(stopA);
        forward.To.Should().Be(stopB);
        backward.From.Should().Be(stopB);
        backward.To.Should().Be(stopA);
        forward.Duration.Should().Be(backward.Duration);
    }

    [Fact]
    public void Reverse_ShouldSwapFromAndTo()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");
        var footpath = Footpath.Create(stopA, stopB, 5);

        // Act
        var reversed = footpath.Reverse();

        // Assert
        reversed.From.Should().Be(stopB);
        reversed.To.Should().Be(stopA);
        reversed.Duration.Should().Be(footpath.Duration);
    }

    [Fact]
    public void CalculatedDistanceMeters_WithCoordinates_ShouldReturnDistance()
    {
        // Arrange - Approximately 1km apart
        var stopA = CreateStop("A", "Stop A", 52.5200, 13.4050);  // Berlin coordinates
        var stopB = CreateStop("B", "Stop B", 52.5290, 13.4050);  // ~1km north

        var footpath = Footpath.Create(stopA, stopB, 12);

        // Act
        var distance = footpath.CalculatedDistanceMeters();

        // Assert
        distance.Should().NotBeNull();
        distance.Should().BeApproximately(1000, 50);  // ~1000m with 50m tolerance
    }

    [Fact]
    public void CalculatedDistanceMeters_WithoutCoordinates_ShouldReturnNull()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");  // No coordinates
        var stopB = CreateStop("B", "Stop B");  // No coordinates
        var footpath = Footpath.Create(stopA, stopB, 5);

        // Act
        var distance = footpath.CalculatedDistanceMeters();

        // Assert
        distance.Should().BeNull();
    }

    [Fact]
    public void IsDurationReasonable_WithReasonableDuration_ShouldReturnTrue()
    {
        // Arrange - Stops ~1km apart, 12 minute walk is reasonable (5 km/h)
        var stopA = CreateStop("A", "Stop A", 52.5200, 13.4050);
        var stopB = CreateStop("B", "Stop B", 52.5290, 13.4050);

        var footpath = new Footpath(
            stopA,
            stopB,
            Duration.FromMinutes(12));

        // Act
        var isReasonable = footpath.IsDurationReasonable();

        // Assert
        isReasonable.Should().BeTrue();
    }

    [Fact]
    public void IsDurationReasonable_WithoutCoordinates_ShouldReturnNull()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");
        var footpath = Footpath.Create(stopA, stopB, 5);

        // Act
        var isReasonable = footpath.IsDurationReasonable();

        // Assert
        isReasonable.Should().BeNull();
    }

    [Fact]
    public void Equality_FootpathsWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        var footpath1 = Footpath.Create(stopA, stopB, 5);
        var footpath2 = Footpath.Create(stopA, stopB, 5);

        // Act & Assert
        footpath1.Should().Be(footpath2);
        (footpath1 == footpath2).Should().BeTrue();
    }

    [Fact]
    public void Equality_FootpathsWithDifferentDuration_ShouldNotBeEqual()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        var footpath1 = Footpath.Create(stopA, stopB, 5);
        var footpath2 = Footpath.Create(stopA, stopB, 10);

        // Act & Assert
        footpath1.Should().NotBe(footpath2);
        (footpath1 != footpath2).Should().BeTrue();
    }

    [Fact]
    public void Equality_FootpathsWithSwappedStops_ShouldNotBeEqual()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        var footpath1 = Footpath.Create(stopA, stopB, 5);
        var footpath2 = Footpath.Create(stopB, stopA, 5);

        // Act & Assert
        footpath1.Should().NotBe(footpath2);
    }

    [Fact]
    public void WheelchairAccessible_ShouldBeSetCorrectly()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        // Act
        var footpath = new Footpath(stopA, stopB, Duration.FromMinutes(5), wheelchairAccessible: true);

        // Assert
        footpath.WheelchairAccessible.Should().BeTrue();
    }

    [Fact]
    public void WheelchairAccessible_DefaultShouldBeFalse()
    {
        // Arrange
        var stopA = CreateStop("A", "Stop A");
        var stopB = CreateStop("B", "Stop B");

        // Act
        var footpath = Footpath.Create(stopA, stopB, 5);

        // Assert
        footpath.WheelchairAccessible.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldIncludeStopNamesAndDuration()
    {
        // Arrange
        var stopA = CreateStop("A", "Central Station");
        var stopB = CreateStop("B", "Market Square");
        var footpath = Footpath.Create(stopA, stopB, 5);

        // Act
        var result = footpath.ToString();

        // Assert
        result.Should().Contain("Central Station");
        result.Should().Contain("Market Square");
        result.Should().Contain("5m");
    }

    [Fact]
    public void ZeroDuration_ShouldBeAllowed()
    {
        // Arrange - Same platform transfers might be instant
        var stopA = CreateStop("A", "Platform 1");
        var stopB = CreateStop("B", "Platform 2");

        // Act
        var footpath = new Footpath(stopA, stopB, Duration.Zero);

        // Assert
        footpath.Duration.TotalMinutes.Should().Be(0);
    }
}
