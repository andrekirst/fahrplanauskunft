using Fahrplanauskunft.Core.Domain;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class StationTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesStation()
    {
        // Arrange
        const string id = "HBF";
        const string name = "Hamburg Hauptbahnhof";

        // Act
        var station = new Station(id, name);

        // Assert
        station.Id.Should().Be(id);
        station.Name.Should().Be(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        // Act
        var act = () => new Station(invalidId!, "Valid Name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Station("ValidId", invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Equals_WithSameId_ReturnsTrue()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("HBF", "Different Name");

        // Act & Assert
        station1.Equals(station2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("ALT", "Altona");

        // Act & Assert
        station1.Equals(station2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var station = new Station("HBF", "Hamburg Hauptbahnhof");

        // Act & Assert
        station.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameId_ReturnsSameHashCode()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("HBF", "Different Name");

        // Act & Assert
        station1.GetHashCode().Should().Be(station2.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_WithSameId_ReturnsTrue()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("HBF", "Different Name");

        // Act & Assert
        (station1 == station2).Should().BeTrue();
        (station1 != station2).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("ALT", "Altona");

        // Act & Assert
        (station1 == station2).Should().BeFalse();
        (station1 != station2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithNullLeft_ReturnsFalseUnlessRightIsNull()
    {
        // Arrange
        Station? nullStation = null;
        var station = new Station("HBF", "Hamburg Hauptbahnhof");

        // Act & Assert
        (nullStation == station).Should().BeFalse();
        (nullStation != station).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        Station? station1 = null;
        Station? station2 = null;

        // Act & Assert
        (station1 == station2).Should().BeTrue();
        (station1 != station2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        // Arrange
        var station = new Station("HBF", "Hamburg Hauptbahnhof");

        // Act & Assert
        station.Equals(station).Should().BeTrue();
    }

    [Fact]
    public void IEquatable_Equals_WithSameId_ReturnsTrue()
    {
        // Arrange
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("HBF", "Different Name");

        // Act - Call the strongly-typed Equals method directly
        bool result = station1.Equals(station2);

        // Assert - This tests the IEquatable<Station>.Equals implementation
        result.Should().BeTrue();
        (station1 is IEquatable<Station>).Should().BeTrue("Station should implement IEquatable<Station>");
    }
}
