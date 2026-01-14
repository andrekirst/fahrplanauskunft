using System.Globalization;
using Bogus;
using NSubstitute;
using Fahrplanauskunft.Core.Domain;
using Fahrplanauskunft.Core.Ports;
using Fahrplanauskunft.Application.UseCases;

namespace Fahrplanauskunft.Tests.Unit.Application;

/// <summary>
/// Tests for GetStationByIdUseCase using NSubstitute for mocking.
/// Demonstrates proper use of the NSubstitute package.
/// </summary>
public class GetStationByIdUseCaseWithMockTests
{
    private readonly IStationRepository _mockRepository;
    private readonly GetStationByIdUseCase _useCase;
    private readonly Faker _faker;

    public GetStationByIdUseCaseWithMockTests()
    {
        _mockRepository = Substitute.For<IStationRepository>();
        _useCase = new GetStationByIdUseCase(_mockRepository);
        _faker = new Faker("de");
    }

    [Fact]
    public void Execute_WithExistingStation_ReturnsStationFromRepository()
    {
        // Arrange
        var stationId = _faker.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture);
        var stationName = _faker.Address.City() + " Hauptbahnhof";
        var expectedStation = new Station(stationId, stationName);

        _mockRepository.GetById(stationId).Returns(expectedStation);

        // Act
        var result = _useCase.Execute(stationId);

        // Assert
        result.Should().Be(expectedStation);
        _mockRepository.Received(1).GetById(stationId);
    }

    [Fact]
    public void Execute_WithNonExistingStation_ReturnsNull()
    {
        // Arrange
        var stationId = _faker.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture);
        _mockRepository.GetById(stationId).Returns((Station?)null);

        // Act
        var result = _useCase.Execute(stationId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Received(1).GetById(stationId);
    }

    [Fact]
    public void Execute_VerifyRepositoryIsCalledWithCorrectId()
    {
        // Arrange
        var stationId = "TEST123";
        _mockRepository.GetById(Arg.Any<string>()).Returns((Station?)null);

        // Act
        _useCase.Execute(stationId);

        // Assert
        _mockRepository.Received(1).GetById(Arg.Is<string>(s => s == stationId));
    }

    [Fact]
    public void Execute_WithBogusGeneratedData_WorksCorrectly()
    {
        // Arrange - Generate multiple fake stations using Bogus
        var stationFaker = new Faker<Station>()
            .CustomInstantiator(f => new Station(
                f.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture),
                f.Address.City() + " " + f.PickRandom("Hbf", "Süd", "Nord", "West", "Ost")));

        var stations = stationFaker.Generate(5);
        var targetStation = stations[2]; // Pick one to find

        _mockRepository.GetById(targetStation.Id).Returns(targetStation);

        // Act
        var result = _useCase.Execute(targetStation.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetStation.Id);
        result.Name.Should().Be(targetStation.Name);
    }

    [Fact]
    public void Execute_DoesNotCallGetAll()
    {
        // Arrange
        var stationId = _faker.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture);
        _mockRepository.GetById(stationId).Returns((Station?)null);

        // Act
        _useCase.Execute(stationId);

        // Assert - Verify GetAll was never called (good isolation)
        _mockRepository.DidNotReceive().GetAll();
    }

    [Fact]
    public void Execute_DoesNotCallAdd()
    {
        // Arrange
        var stationId = _faker.Random.AlphaNumeric(5).ToUpper(CultureInfo.InvariantCulture);
        _mockRepository.GetById(stationId).Returns((Station?)null);

        // Act
        _useCase.Execute(stationId);

        // Assert - Verify Add was never called (read-only operation)
        _mockRepository.DidNotReceive().Add(Arg.Any<Station>());
    }

    [Theory]
    [InlineData("HBF")]
    [InlineData("ALT")]
    [InlineData("DAMMTOR")]
    public void Execute_MultipleStationIds_CallsRepositoryWithEachId(string stationId)
    {
        // Arrange
        var expectedStation = new Station(stationId, $"{stationId} Station");
        _mockRepository.GetById(stationId).Returns(expectedStation);

        // Act
        var result = _useCase.Execute(stationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(stationId);
    }
}
