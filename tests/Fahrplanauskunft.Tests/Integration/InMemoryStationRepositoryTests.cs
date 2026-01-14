using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Infrastructure.Adapters;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Integration;

public class InMemoryStationRepositoryTests
{
    [Fact]
    public void Add_AndGetById_ReturnsAddedStation()
    {
        // Arrange
        var repository = new InMemoryStationRepository();
        var station = new Station("HBF", "Hamburg Hauptbahnhof");

        // Act
        repository.Add(station);
        var result = repository.GetById("HBF");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(station);
    }

    [Fact]
    public void GetById_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryStationRepository();

        // Act
        var result = repository.GetById("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_WithMultipleStations_ReturnsAllStations()
    {
        // Arrange
        var repository = new InMemoryStationRepository();
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("ALT", "Altona");
        var station3 = new Station("DAM", "Dammtor");

        repository.Add(station1);
        repository.Add(station2);
        repository.Add(station3);

        // Act
        var result = repository.GetAll().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(station1);
        result.Should().Contain(station2);
        result.Should().Contain(station3);
    }

    [Fact]
    public void GetAll_WithNoStations_ReturnsEmptyCollection()
    {
        // Arrange
        var repository = new InMemoryStationRepository();

        // Act
        var result = repository.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Add_WithNullStation_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryStationRepository();

        // Act
        var act = () => repository.Add(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("station");
    }

    [Fact]
    public void Add_WithSameId_OverwritesExistingStation()
    {
        // Arrange
        var repository = new InMemoryStationRepository();
        var station1 = new Station("HBF", "Hamburg Hauptbahnhof");
        var station2 = new Station("HBF", "Hamburg Hbf (Updated)");

        // Act
        repository.Add(station1);
        repository.Add(station2);
        var result = repository.GetById("HBF");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Hamburg Hbf (Updated)");
    }
}
