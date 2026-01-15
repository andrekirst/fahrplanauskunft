using Bogus;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Fahrplanauskunft.Infrastructure.Data;
using Fahrplanauskunft.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fahrplanauskunft.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for StopRepository.
/// Uses SQLite in-memory database for testing EF Core operations.
/// </summary>
public class StopRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FahrplanDbContext> _options;
    private readonly ILogger<StopRepository> _logger;
    private readonly Faker _faker;

    public StopRepositoryTests()
    {
        // SQLite in-memory requires the connection to stay open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<FahrplanDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema
        using var context = CreateContext();
        context.Database.EnsureCreated();

        _logger = Substitute.For<ILogger<StopRepository>>();
        _faker = new Faker("de");
    }

    private FahrplanDbContext CreateContext() => new(_options);

    private StopRepository CreateRepository(FahrplanDbContext context) => new(context, _logger);

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StopRepository(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var act = () => new StopRepository(context, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var repository = new StopRepository(context, _logger);

        // Assert
        repository.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingStop_ReturnsSuccessWithStop()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_001");
        var stop = new Stop(stopId, "Hamburg Hauptbahnhof");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(stopId);
        result.Value.Name.Should().Be("Hamburg Hauptbahnhof");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingStop_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stopId = StopId.From("non_existing_stop");

        // Act
        var result = await repository.GetByIdAsync(stopId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("non_existing_stop");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stopId = StopId.From("stop_001");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetByIdAsync(stopId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithStopHavingAllProperties_ReturnsCompleteStop()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_full");
        var coordinates = new Coordinates(53.552676, 9.993429); // Hamburg
        var stop = new Stop(stopId, "Hamburg Dammtor", coordinates, "Platform 5", true);
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Hamburg Dammtor");
        result.Value.Location.Should().NotBeNull();
        result.Value.Location!.Value.Latitude.Should().BeApproximately(53.552676, 0.000001);
        result.Value.Location!.Value.Longitude.Should().BeApproximately(9.993429, 0.000001);
        result.Value.PlatformCode.Should().Be("Platform 5");
        result.Value.WheelchairAccessible.Should().BeTrue();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoStops_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleStops_ReturnsAllStops()
    {
        // Arrange
        using var context = CreateContext();
        var stops = new[]
        {
            new Stop(StopId.From("stop_1"), "Hamburg Hauptbahnhof"),
            new Stop(StopId.From("stop_2"), "Hamburg Dammtor"),
            new Stop(StopId.From("stop_3"), "Hamburg Altona"),
        };
        context.Stops.AddRange(stops);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Select(s => s.Name).Should().Contain("Hamburg Hauptbahnhof");
        result.Value.Select(s => s.Name).Should().Contain("Hamburg Dammtor");
        result.Value.Select(s => s.Name).Should().Contain("Hamburg Altona");
    }

    [Fact]
    public async Task GetAllAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetAllAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsReadOnlyList()
    {
        // Arrange
        using var context = CreateContext();
        var stop = new Stop(StopId.From("stop_readonly"), "Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IReadOnlyList<Stop>>();
    }

    #endregion

    #region SearchByNameAsync Tests

    [Fact]
    public async Task SearchByNameAsync_WithNullSearchTerm_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Search term cannot be null or empty");
    }

    [Fact]
    public async Task SearchByNameAsync_WithEmptySearchTerm_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Search term cannot be null or empty");
    }

    [Fact]
    public async Task SearchByNameAsync_WithWhitespaceSearchTerm_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Search term cannot be null or empty");
    }

    [Fact]
    public async Task SearchByNameAsync_WithMatchingTerm_ReturnsMatchingStops()
    {
        // Arrange
        using var context = CreateContext();
        var stops = new[]
        {
            new Stop(StopId.From("stop_hh_1"), "Hamburg Hauptbahnhof"),
            new Stop(StopId.From("stop_hh_2"), "Hamburg Dammtor"),
            new Stop(StopId.From("stop_b_1"), "Berlin Hauptbahnhof"),
        };
        context.Stops.AddRange(stops);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync("Hamburg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(s => s.Name.Contains("Hamburg")).Should().BeTrue();
    }

    [Fact]
    public async Task SearchByNameAsync_WithPartialMatch_ReturnsMatchingStops()
    {
        // Arrange
        using var context = CreateContext();
        var stops = new[]
        {
            new Stop(StopId.From("stop_1"), "Hamburg Hauptbahnhof"),
            new Stop(StopId.From("stop_2"), "Berlin Hauptbahnhof"),
            new Stop(StopId.From("stop_3"), "München Hauptbahnhof"),
        };
        context.Stops.AddRange(stops);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync("Hauptbahnhof");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchByNameAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var stop = new Stop(StopId.From("stop_1"), "Hamburg Hauptbahnhof");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync("Frankfurt");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByNameAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.SearchByNameAsync("test", cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SearchByNameAsync_IsCaseInsensitive()
    {
        // Arrange
        using var context = CreateContext();
        var stop = new Stop(StopId.From("stop_1"), "Hamburg Hauptbahnhof");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var resultLower = await repository.SearchByNameAsync("hamburg");
        var resultUpper = await repository.SearchByNameAsync("HAMBURG");
        var resultMixed = await repository.SearchByNameAsync("HaMbUrG");

        // Assert
        resultLower.IsSuccess.Should().BeTrue();
        resultUpper.IsSuccess.Should().BeTrue();
        resultMixed.IsSuccess.Should().BeTrue();
        resultLower.Value.Should().HaveCount(1);
        resultUpper.Value.Should().HaveCount(1);
        resultMixed.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchByNameAsync_WithSpecialSearchTerm_ReturnsResults()
    {
        // Arrange
        using var context = CreateContext();
        var stops = new[]
        {
            new Stop(StopId.From("stop_special_1"), "München Hauptbahnhof"),
            new Stop(StopId.From("stop_special_2"), "Köln Hbf"),
            new Stop(StopId.From("stop_special_3"), "Frankfurt (Main)"),
        };
        context.Stops.AddRange(stops);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act - searching with special characters (parentheses)
        var resultParens = await repository.SearchByNameAsync("(Main)");
        var resultUmlaut = await repository.SearchByNameAsync("München");

        // Assert
        resultParens.IsSuccess.Should().BeTrue();
        resultParens.Value.Should().HaveCount(1);
        resultParens.Value[0].Name.Should().Be("Frankfurt (Main)");

        resultUmlaut.IsSuccess.Should().BeTrue();
        resultUmlaut.Value.Should().HaveCount(1);
        resultUmlaut.Value[0].Name.Should().Be("München Hauptbahnhof");
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_WithNullCollection_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.AddRangeAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Stops collection cannot be null");
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.AddRangeAsync(Array.Empty<Stop>());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_WithValidStops_AddsStopsToDatabase()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stops = new[]
        {
            new Stop(StopId.From("add_stop_1"), "Hamburg Hauptbahnhof"),
            new Stop(StopId.From("add_stop_2"), "Hamburg Dammtor"),
        };

        // Act
        var result = await repository.AddRangeAsync(stops);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify stops were added
        using var verifyContext = CreateContext();
        var count = await verifyContext.Stops.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task AddRangeAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stops = new[] { new Stop(StopId.From("stop_cancel"), "Test Station") };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.AddRangeAsync(stops, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AddRangeAsync_WithDuplicateIds_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var existingStop = new Stop(StopId.From("duplicate_stop"), "Existing Station");
        context.Stops.Add(existingStop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);
        var newStops = new[] { new Stop(StopId.From("duplicate_stop"), "New Station") };

        // Act
        var result = await repository.AddRangeAsync(newStops);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Error message should indicate failure - either database error or generic error
        result.Error.Should().ContainAny("error", "Error");
    }

    [Fact]
    public async Task AddRangeAsync_WithLargeCollection_AddsAllStops()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stops = Enumerable.Range(1, 100)
            .Select(i => new Stop(StopId.From($"bulk_stop_{i}"), $"Station {i}"))
            .ToList();

        // Act
        var result = await repository.AddRangeAsync(stops);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Stops.CountAsync();
        count.Should().Be(100);
    }

    [Fact]
    public async Task AddRangeAsync_WithStopsHavingAllProperties_PersistsAllProperties()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stopId = StopId.From("full_props_stop");
        var coordinates = new Coordinates(53.552676, 9.993429);
        var stops = new[]
        {
            new Stop(stopId, "Hamburg Dammtor", coordinates, "Platform 5", true)
        };

        // Act
        var result = await repository.AddRangeAsync(stops);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var loaded = await verifyContext.Stops.FirstAsync(s => s.Id == stopId);
        loaded.Name.Should().Be("Hamburg Dammtor");
        loaded.Location.Should().NotBeNull();
        loaded.Location!.Value.Latitude.Should().BeApproximately(53.552676, 0.000001);
        loaded.PlatformCode.Should().Be("Platform 5");
        loaded.WheelchairAccessible.Should().BeTrue();
    }

    #endregion

    #region AsNoTracking Verification Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsUntrackedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("untracked_stop");
        var stop = new Stop(stopId, "Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Entry(result.Value).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUntrackedEntities()
    {
        // Arrange
        using var context = CreateContext();
        var stop = new Stop(StopId.From("untracked_all"), "Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var s in result.Value)
        {
            context.Entry(s).State.Should().Be(EntityState.Detached);
        }
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsUntrackedEntities()
    {
        // Arrange
        using var context = CreateContext();
        var stop = new Stop(StopId.From("untracked_search"), "Test Station");
        context.Stops.Add(stop);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.SearchByNameAsync("Test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var s in result.Value)
        {
            context.Entry(s).State.Should().Be(EntityState.Detached);
        }
    }

    #endregion

    #region Bogus Data Tests

    [Fact]
    public async Task GetByIdAsync_WithBogusGeneratedStop_WorksCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var stopFaker = new Faker<Stop>()
            .CustomInstantiator(f => new Stop(
                StopId.From($"bogus_stop_{f.Random.AlphaNumeric(5)}"),
                f.Address.City() + " " + f.PickRandom("Hbf", "Süd", "Nord", "West", "Ost")));

        var fakeStop = stopFaker.Generate();
        context.Stops.Add(fakeStop);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(fakeStop.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(fakeStop.Name);
    }

    [Fact]
    public async Task AddRangeAsync_WithBogusGeneratedStops_AddsAllStops()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var counter = 0;
        var stopFaker = new Faker<Stop>()
            .CustomInstantiator(f =>
            {
                counter++;
                return new Stop(
                    StopId.From($"bogus_bulk_{counter}"),
                    f.Address.City() + " " + f.PickRandom("Hbf", "Süd", "Nord"));
            });

        var fakeStops = stopFaker.Generate(10);

        // Act
        var result = await repository.AddRangeAsync(fakeStops);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Stops.CountAsync();
        count.Should().Be(10);
    }

    #endregion
}
