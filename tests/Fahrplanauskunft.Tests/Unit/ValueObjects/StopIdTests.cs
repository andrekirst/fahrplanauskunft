using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class StopIdTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("stop_123")]
    [InlineData("S1")]
    [InlineData("de:09162:6")]
    public void Constructor_WithValidId_CreatesStopId(string value)
    {
        // Act
        var stopId = new StopId(value);

        // Assert
        stopId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespace_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => new StopId(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidValue_CreatesStopId()
    {
        // Act
        var stopId = StopId.From("stop_123");

        // Assert
        stopId.Value.Should().Be("stop_123");
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = StopId.TryCreate("stop_123", out var stopId);

        // Assert
        result.Should().BeTrue();
        stopId.Value.Should().Be("stop_123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_WithInvalidValue_ReturnsFalse(string? value)
    {
        // Act
        var result = StopId.TryCreate(value, out var stopId);

        // Assert
        result.Should().BeFalse();
        stopId.Value.Should().BeNull(); // Default struct has null Value
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var stopId1 = new StopId("stop_123");
        var stopId2 = new StopId("stop_123");

        // Act & Assert
        stopId1.Equals(stopId2).Should().BeTrue();
        (stopId1 == stopId2).Should().BeTrue();
        (stopId1 != stopId2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var stopId1 = new StopId("stop_123");
        var stopId2 = new StopId("stop_456");

        // Act & Assert
        stopId1.Equals(stopId2).Should().BeFalse();
        (stopId1 == stopId2).Should().BeFalse();
        (stopId1 != stopId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_CaseSensitive_ReturnsFalse()
    {
        // Arrange
        var stopId1 = new StopId("Stop_123");
        var stopId2 = new StopId("stop_123");

        // Act & Assert
        stopId1.Equals(stopId2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var stopId1 = new StopId("stop_123");
        var stopId2 = new StopId("stop_123");

        // Act & Assert
        stopId1.GetHashCode().Should().Be(stopId2.GetHashCode());
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitToString_ReturnsValue()
    {
        // Arrange
        var stopId = new StopId("stop_123");

        // Act
        var result = (string)stopId;

        // Assert
        result.Should().Be("stop_123");
    }

    [Fact]
    public void ExplicitFromString_CreatesStopId()
    {
        // Act
        var stopId = (StopId)"stop_123";

        // Assert
        stopId.Value.Should().Be("stop_123");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var stopId = new StopId("stop_123");

        // Act
        var result = stopId.ToString();

        // Assert
        result.Should().Be("stop_123");
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Default_ToString_ReturnsEmptyString()
    {
        // Arrange
        var stopId = default(StopId);

        // Act
        var result = stopId.ToString();

        // Assert
        result.Should().Be(string.Empty);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void DifferentIdTypes_AreNotEqual()
    {
        // This test verifies that StopId, RouteId, and TripId are not interchangeable
        // at runtime (they are different types at compile time)

        // Arrange
        var stopId = new StopId("123");
        var routeId = new RouteId("123");
        var tripId = new TripId("123");

        // Act & Assert
        // These should all be different types
        stopId.GetType().Should().NotBe(routeId.GetType());
        stopId.GetType().Should().NotBe(tripId.GetType());
        routeId.GetType().Should().NotBe(tripId.GetType());
    }

    #endregion
}
