using Fahrplanauskunft.Application.UseCases;
using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Core.Ports;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.Application;

public class GetStationByIdUseCaseTests
{
    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new GetStationByIdUseCase(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stationRepository");
    }

    [Fact]
    public void Execute_WithExistingStation_ReturnsStation()
    {
        // Arrange
        var expectedStation = new Station("HBF", "Hamburg Hauptbahnhof");
        var repository = new FakeStationRepository();
        repository.AddStation(expectedStation);
        var useCase = new GetStationByIdUseCase(repository);

        // Act
        var result = useCase.Execute("HBF");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedStation);
    }

    [Fact]
    public void Execute_WithNonExistingStation_ReturnsNull()
    {
        // Arrange
        var repository = new FakeStationRepository();
        var useCase = new GetStationByIdUseCase(repository);

        // Act
        var result = useCase.Execute("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Execute_WithInvalidStationId_ThrowsArgumentException(string? invalidId)
    {
        // Arrange
        var repository = new FakeStationRepository();
        var useCase = new GetStationByIdUseCase(repository);

        // Act
        var act = () => useCase.Execute(invalidId!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("stationId")
            .WithMessage("*cannot be empty*");
    }

    /// <summary>
    /// Test double implementing IStationRepository for unit testing.
    /// </summary>
    private sealed class FakeStationRepository : IStationRepository
    {
        private readonly Dictionary<string, Station> _stations = new();

        public void AddStation(Station station) => _stations[station.Id] = station;

        public Station? GetById(string id) => _stations.GetValueOrDefault(id);

        public IEnumerable<Station> GetAll() => _stations.Values;

        public void Add(Station station) => _stations[station.Id] = station;
    }
}
