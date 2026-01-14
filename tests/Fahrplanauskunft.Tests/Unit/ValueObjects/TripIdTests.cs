using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class TripIdTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("trip_12345")]
    [InlineData("S1_08:30")]
    [InlineData("de:mvv:1:1234")]
    public void Constructor_WithValidId_CreatesTripId(string value)
    {
        // Act
        var tripId = new TripId(value);

        // Assert
        tripId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespace_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => new TripId(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidValue_CreatesTripId()
    {
        // Act
        var tripId = TripId.From("trip_12345");

        // Assert
        tripId.Value.Should().Be("trip_12345");
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = TripId.TryCreate("trip_12345", out var tripId);

        // Assert
        result.Should().BeTrue();
        tripId.Value.Should().Be("trip_12345");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_WithInvalidValue_ReturnsFalse(string? value)
    {
        // Act
        var result = TripId.TryCreate(value, out var tripId);

        // Assert
        result.Should().BeFalse();
        tripId.Value.Should().BeNull(); // Default struct has null Value
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var tripId1 = new TripId("trip_12345");
        var tripId2 = new TripId("trip_12345");

        // Act & Assert
        tripId1.Equals(tripId2).Should().BeTrue();
        (tripId1 == tripId2).Should().BeTrue();
        (tripId1 != tripId2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var tripId1 = new TripId("trip_12345");
        var tripId2 = new TripId("trip_67890");

        // Act & Assert
        tripId1.Equals(tripId2).Should().BeFalse();
        (tripId1 == tripId2).Should().BeFalse();
        (tripId1 != tripId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var tripId1 = new TripId("trip_12345");
        var tripId2 = new TripId("trip_12345");

        // Act & Assert
        tripId1.GetHashCode().Should().Be(tripId2.GetHashCode());
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Default_ToString_ReturnsEmptyString()
    {
        // Arrange
        var tripId = default(TripId);

        // Act
        var result = tripId.ToString();

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Default_Value_IsNull()
    {
        // Arrange
        var tripId = default(TripId);

        // Assert
        tripId.Value.Should().BeNull();
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitToString_ReturnsValue()
    {
        // Arrange
        var tripId = new TripId("trip_12345");

        // Act
        var result = (string)tripId;

        // Assert
        result.Should().Be("trip_12345");
    }

    [Fact]
    public void ExplicitFromString_CreatesTripId()
    {
        // Act
        var tripId = (TripId)"trip_12345";

        // Assert
        tripId.Value.Should().Be("trip_12345");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var tripId = new TripId("trip_12345");

        // Act
        var result = tripId.ToString();

        // Assert
        result.Should().Be("trip_12345");
    }

    #endregion
}
