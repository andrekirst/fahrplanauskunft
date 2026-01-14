using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class CoordinatesTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(48.1374, 11.5755)] // Munich
    [InlineData(52.5200, 13.4050)] // Berlin
    [InlineData(-33.8688, 151.2093)] // Sydney
    [InlineData(90.0, 180.0)] // North pole, date line
    [InlineData(-90.0, -180.0)] // South pole, date line
    public void Constructor_WithValidCoordinates_CreatesCoordinates(double latitude, double longitude)
    {
        // Act
        var coordinates = new Coordinates(latitude, longitude);

        // Assert
        coordinates.Latitude.Should().Be(latitude);
        coordinates.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-90.1, 0.0)]
    [InlineData(90.1, 0.0)]
    [InlineData(-100, 0.0)]
    [InlineData(100, 0.0)]
    public void Constructor_WithInvalidLatitude_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => new Coordinates(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(0.0, -180.1)]
    [InlineData(0.0, 180.1)]
    [InlineData(0.0, -200)]
    [InlineData(0.0, 200)]
    public void Constructor_WithInvalidLongitude_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => new Coordinates(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Theory]
    [InlineData(double.NaN, 0.0)]
    [InlineData(double.PositiveInfinity, 0.0)]
    [InlineData(double.NegativeInfinity, 0.0)]
    public void Constructor_WithNaNOrInfinityLatitude_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => new Coordinates(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(0.0, double.NaN)]
    [InlineData(0.0, double.PositiveInfinity)]
    [InlineData(0.0, double.NegativeInfinity)]
    public void Constructor_WithNaNOrInfinityLongitude_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => new Coordinates(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidCoordinates_CreatesCoordinates()
    {
        // Act
        var coordinates = Coordinates.From(48.1374, 11.5755);

        // Assert
        coordinates.Latitude.Should().Be(48.1374);
        coordinates.Longitude.Should().Be(11.5755);
    }

    [Fact]
    public void TryCreate_WithValidCoordinates_ReturnsTrue()
    {
        // Act
        var result = Coordinates.TryCreate(48.1374, 11.5755, out var coordinates);

        // Assert
        result.Should().BeTrue();
        coordinates.Latitude.Should().Be(48.1374);
        coordinates.Longitude.Should().Be(11.5755);
    }

    [Fact]
    public void TryCreate_WithInvalidLatitude_ReturnsFalse()
    {
        // Act
        var result = Coordinates.TryCreate(100, 11.5755, out var coordinates);

        // Assert
        result.Should().BeFalse();
        coordinates.Latitude.Should().Be(0); // Default struct has 0 values
        coordinates.Longitude.Should().Be(0);
    }

    [Fact]
    public void TryCreate_WithInvalidLongitude_ReturnsFalse()
    {
        // Act
        var result = Coordinates.TryCreate(48.1374, 200, out var coordinates);

        // Assert
        result.Should().BeFalse();
        coordinates.Latitude.Should().Be(0); // Default struct has 0 values
        coordinates.Longitude.Should().Be(0);
    }

    #endregion

    #region Distance Calculation Tests

    [Fact]
    public void DistanceToKm_SameLocation_ReturnsZero()
    {
        // Arrange
        var coordinates = new Coordinates(48.1374, 11.5755);

        // Act
        var distance = coordinates.DistanceToKm(coordinates);

        // Assert
        distance.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void DistanceToKm_MunichToBerlin_ReturnsApproximatelyCorrectDistance()
    {
        // Arrange
        var munich = new Coordinates(48.1374, 11.5755);
        var berlin = new Coordinates(52.5200, 13.4050);

        // Act
        var distance = munich.DistanceToKm(berlin);

        // Assert - Munich to Berlin is approximately 504 km
        distance.Should().BeApproximately(504, 5);
    }

    [Fact]
    public void DistanceToKm_IsSymmetric()
    {
        // Arrange
        var munich = new Coordinates(48.1374, 11.5755);
        var berlin = new Coordinates(52.5200, 13.4050);

        // Act
        var distanceMunichToBerlin = munich.DistanceToKm(berlin);
        var distanceBerlinToMunich = berlin.DistanceToKm(munich);

        // Assert
        distanceMunichToBerlin.Should().BeApproximately(distanceBerlinToMunich, 0.001);
    }

    [Fact]
    public void DistanceToMeters_ReturnsDistanceInMeters()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1380, 11.5760);

        // Act
        var distanceKm = coord1.DistanceToKm(coord2);
        var distanceMeters = coord1.DistanceToMeters(coord2);

        // Assert
        distanceMeters.Should().BeApproximately(distanceKm * 1000, 0.1);
    }

    #endregion

    #region Within Distance Tests

    [Fact]
    public void IsWithinKm_WhenWithinDistance_ReturnsTrue()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1380, 11.5760);

        // Act
        var result = coord1.IsWithinKm(coord2, 1.0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinKm_WhenOutsideDistance_ReturnsFalse()
    {
        // Arrange
        var munich = new Coordinates(48.1374, 11.5755);
        var berlin = new Coordinates(52.5200, 13.4050);

        // Act
        var result = munich.IsWithinKm(berlin, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinMeters_WhenWithinDistance_ReturnsTrue()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1375, 11.5756);

        // Act
        var result = coord1.IsWithinMeters(coord2, 500);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinMeters_WhenOutsideDistance_ReturnsFalse()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1400, 11.5800); // ~300m away

        // Act
        var result = coord1.IsWithinMeters(coord2, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinKm_SameLocation_ReturnsTrue()
    {
        // Arrange
        var coord = new Coordinates(48.1374, 11.5755);

        // Act
        var result = coord.IsWithinKm(coord, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DistanceToKm_Antipodal_ReturnsApproximatelyHalfEarthCircumference()
    {
        // Arrange - roughly antipodal points
        var northPole = new Coordinates(90, 0);
        var southPole = new Coordinates(-90, 0);

        // Act
        var distance = northPole.DistanceToKm(southPole);

        // Assert - Half of Earth's circumference is approximately 20,015 km
        distance.Should().BeApproximately(20015, 100);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1374, 11.5755);

        // Act & Assert
        coord1.Equals(coord2).Should().BeTrue();
        (coord1 == coord2).Should().BeTrue();
        (coord1 != coord2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(52.5200, 13.4050);

        // Act & Assert
        coord1.Equals(coord2).Should().BeFalse();
        (coord1 == coord2).Should().BeFalse();
        (coord1 != coord2).Should().BeTrue();
    }

    [Fact]
    public void Equals_VerySmallDifference_ReturnsTrue()
    {
        // Arrange - difference smaller than tolerance
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.13740000001, 11.57550000001);

        // Act & Assert
        coord1.Equals(coord2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameCoordinates_ReturnsSameHashCode()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        var coord2 = new Coordinates(48.1374, 11.5755);

        // Act & Assert
        coord1.GetHashCode().Should().Be(coord2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        var coord = new Coordinates(48.1374, 11.5755);

        // Act & Assert
        coord.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var coord = new Coordinates(48.1374, 11.5755);
        object differentType = "48.1374,11.5755";

        // Act & Assert
        coord.Equals(differentType).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithBoxedCoordinates_ReturnsTrue()
    {
        // Arrange
        var coord1 = new Coordinates(48.1374, 11.5755);
        object boxedCoord = new Coordinates(48.1374, 11.5755);

        // Act & Assert
        coord1.Equals(boxedCoord).Should().BeTrue();
    }

    [Fact]
    public void Default_Coordinates_HasZeroValues()
    {
        // Arrange
        var coord = default(Coordinates);

        // Assert
        coord.Latitude.Should().Be(0);
        coord.Longitude.Should().Be(0);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsLatLonFormat()
    {
        // Arrange
        var coordinates = new Coordinates(48.137400, 11.575500);

        // Act
        var result = coordinates.ToString();

        // Assert
        result.Should().Be("48.137400,11.575500");
    }

    [Fact]
    public void ToIso6709String_NorthEast_ReturnsCorrectFormat()
    {
        // Arrange
        var coordinates = new Coordinates(48.137400, 11.575500);

        // Act
        var result = coordinates.ToIso6709String();

        // Assert
        result.Should().Be("48.137400N 11.575500E");
    }

    [Fact]
    public void ToIso6709String_SouthWest_ReturnsCorrectFormat()
    {
        // Arrange
        var coordinates = new Coordinates(-33.868800, -71.209300);

        // Act
        var result = coordinates.ToIso6709String();

        // Assert
        result.Should().Be("33.868800S 71.209300W");
    }

    #endregion
}
