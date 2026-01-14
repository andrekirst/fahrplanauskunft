using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class RouteIdTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("route_S1")]
    [InlineData("U6")]
    [InlineData("de:mvv:1")]
    public void Constructor_WithValidId_CreatesRouteId(string value)
    {
        // Act
        var routeId = new RouteId(value);

        // Assert
        routeId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespace_ThrowsArgumentException(string? value)
    {
        // Act
        var act = () => new RouteId(value!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidValue_CreatesRouteId()
    {
        // Act
        var routeId = RouteId.From("route_S1");

        // Assert
        routeId.Value.Should().Be("route_S1");
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = RouteId.TryCreate("route_S1", out var routeId);

        // Assert
        result.Should().BeTrue();
        routeId.Value.Should().Be("route_S1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_WithInvalidValue_ReturnsFalse(string? value)
    {
        // Act
        var result = RouteId.TryCreate(value, out var routeId);

        // Assert
        result.Should().BeFalse();
        routeId.Value.Should().BeNull(); // Default struct has null Value
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var routeId1 = new RouteId("route_S1");
        var routeId2 = new RouteId("route_S1");

        // Act & Assert
        routeId1.Equals(routeId2).Should().BeTrue();
        (routeId1 == routeId2).Should().BeTrue();
        (routeId1 != routeId2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var routeId1 = new RouteId("route_S1");
        var routeId2 = new RouteId("route_U6");

        // Act & Assert
        routeId1.Equals(routeId2).Should().BeFalse();
        (routeId1 == routeId2).Should().BeFalse();
        (routeId1 != routeId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var routeId1 = new RouteId("route_S1");
        var routeId2 = new RouteId("route_S1");

        // Act & Assert
        routeId1.GetHashCode().Should().Be(routeId2.GetHashCode());
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Default_ToString_ReturnsEmptyString()
    {
        // Arrange
        var routeId = default(RouteId);

        // Act
        var result = routeId.ToString();

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Default_Value_IsNull()
    {
        // Arrange
        var routeId = default(RouteId);

        // Assert
        routeId.Value.Should().BeNull();
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitToString_ReturnsValue()
    {
        // Arrange
        var routeId = new RouteId("route_S1");

        // Act
        var result = (string)routeId;

        // Assert
        result.Should().Be("route_S1");
    }

    [Fact]
    public void ExplicitFromString_CreatesRouteId()
    {
        // Act
        var routeId = (RouteId)"route_S1";

        // Assert
        routeId.Value.Should().Be("route_S1");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var routeId = new RouteId("route_S1");

        // Act
        var result = routeId.ToString();

        // Assert
        result.Should().Be("route_S1");
    }

    #endregion
}
