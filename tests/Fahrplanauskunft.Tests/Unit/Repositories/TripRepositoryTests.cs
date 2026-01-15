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
/// Unit tests for TripRepository.
/// Uses SQLite in-memory database for testing EF Core operations.
/// </summary>
public class TripRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FahrplanDbContext> _options;
    private readonly ILogger<TripRepository> _logger;
    private readonly Faker _faker;

    public TripRepositoryTests()
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

        _logger = Substitute.For<ILogger<TripRepository>>();
        _faker = new Faker("de");
    }

    private FahrplanDbContext CreateContext() => new(_options);

    private TripRepository CreateRepository(FahrplanDbContext context) => new(context, _logger);

    /// <summary>
    /// Helper method to insert StopTime via raw SQL to bypass EF Core's navigation property traversal.
    /// This is needed because StopTime has a composite primary key with shadow properties.
    /// </summary>
    private static async Task InsertStopTimeAsync(FahrplanDbContext context, string tripId, string stopId, int sequence, int arrivalMinutes, int departureMinutes)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO StopTimes (TripId, StopId, Sequence, ArrivalMinutes, DepartureMinutes, PickupAllowed, DropOffAllowed) VALUES ({0}, {1}, {2}, {3}, {4}, 1, 1)",
            tripId, stopId, sequence, arrivalMinutes, departureMinutes);
    }

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
        var act = () => new TripRepository(null!, _logger);

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
        var act = () => new TripRepository(context, null!);

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
        var repository = new TripRepository(context, _logger);

        // Assert
        repository.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingTrip_ReturnsSuccessWithTrip()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("trip_001");
        var trip = new Trip(tripId, "Hamburg Hauptbahnhof", "S1");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(tripId);
        result.Value.Headsign.Should().Be("Hamburg Hauptbahnhof");
        result.Value.ShortName.Should().Be("S1");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingTrip_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var tripId = TripId.From("non_existing_trip");

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("non_existing_trip");
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var tripId = TripId.From("trip_001");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetByIdAsync(tripId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithTripHavingAllProperties_ReturnsCompleteTrip()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("trip_full");
        var trip = new Trip(tripId, "Berlin Hauptbahnhof", "ICE");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Headsign.Should().Be("Berlin Hauptbahnhof");
        result.Value.ShortName.Should().Be("ICE");
    }

    [Fact]
    public async Task GetByIdAsync_WithTripHavingNoOptionalProperties_ReturnsTrip()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("trip_minimal");
        var trip = new Trip(tripId);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(tripId);
        result.Value.Headsign.Should().BeNull();
        result.Value.ShortName.Should().BeNull();
    }

    #endregion

    #region GetByRouteAsync Tests

    [Fact]
    public async Task GetByRouteAsync_WithExistingRoute_ReturnsTripsForRoute()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_001");

        var trip1 = new Trip(TripId.From("trip_r1_1"), "Destination A");
        var trip2 = new Trip(TripId.From("trip_r1_2"), "Destination B");
        var trip3 = new Trip(TripId.From("trip_r2_1"), "Other Route");

        context.Trips.Add(trip1);
        context.Trips.Add(trip2);
        context.Trips.Add(trip3);
        await context.SaveChangesAsync();

        // Set RouteId shadow property
        context.Entry(trip1).Property("RouteId").CurrentValue = routeId.Value;
        context.Entry(trip2).Property("RouteId").CurrentValue = routeId.Value;
        context.Entry(trip3).Property("RouteId").CurrentValue = "route_002";
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(t => t.Id.Value).Should().Contain("trip_r1_1");
        result.Value.Select(t => t.Id.Value).Should().Contain("trip_r1_2");
        result.Value.Select(t => t.Id.Value).Should().NotContain("trip_r2_1");
    }

    [Fact]
    public async Task GetByRouteAsync_WithNonExistingRoute_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var trip = new Trip(TripId.From("trip_001"), "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        context.Entry(trip).Property("RouteId").CurrentValue = "route_001";
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);
        var nonExistingRouteId = RouteId.From("non_existing_route");

        // Act
        var result = await repository.GetByRouteAsync(nonExistingRouteId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRouteAsync_WithNoTrips_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("route_001");

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRouteAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var routeId = RouteId.From("route_001");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetByRouteAsync(routeId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByRouteAsync_ReturnsReadOnlyList()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_001");
        var trip = new Trip(TripId.From("trip_readonly"), "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        context.Entry(trip).Property("RouteId").CurrentValue = routeId.Value;
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IReadOnlyList<Trip>>();
    }

    [Fact]
    public async Task GetByRouteAsync_WithMultipleTripsOnRoute_ReturnsAllTrips()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_multi");

        for (int i = 1; i <= 5; i++)
        {
            var trip = new Trip(TripId.From($"trip_multi_{i}"), $"Destination {i}");
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
            context.Entry(trip).Property("RouteId").CurrentValue = routeId.Value;
        }
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    #endregion

    #region GetTripsServingStopAsync Tests

    [Fact]
    public async Task GetTripsServingStopAsync_WithTripsServingStop_ReturnsTrips()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_001");
        var stop = new Stop(stopId, "Hamburg Hauptbahnhof");
        context.Stops.Add(stop);

        var trip1 = new Trip(TripId.From("trip_serving_1"), "Destination A");
        var trip2 = new Trip(TripId.From("trip_serving_2"), "Destination B");
        var trip3 = new Trip(TripId.From("trip_other"), "Other");

        context.Trips.Add(trip1);
        context.Trips.Add(trip2);
        context.Trips.Add(trip3);
        await context.SaveChangesAsync();

        // Create StopTimes linking trips to the stop using raw SQL
        // (StopTime has composite primary key with shadow properties that cause issues with EF Core tracking)
        await InsertStopTimeAsync(context, "trip_serving_1", "stop_001", 1, 480, 480); // 8:00
        await InsertStopTimeAsync(context, "trip_serving_2", "stop_001", 1, 540, 540); // 9:00

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetTripsServingStopAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(t => t.Id.Value).Should().Contain("trip_serving_1");
        result.Value.Select(t => t.Id.Value).Should().Contain("trip_serving_2");
    }

    [Fact]
    public async Task GetTripsServingStopAsync_WithNoTripsServingStop_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_no_trips");
        var stop = new Stop(stopId, "Empty Station");
        context.Stops.Add(stop);

        var trip = new Trip(TripId.From("trip_other"), "Other");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetTripsServingStopAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "the result should succeed but got error: {0}", result.IsFailure ? result.Error : "N/A");
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTripsServingStopAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var stopId = StopId.From("stop_001");
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.GetTripsServingStopAsync(stopId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetTripsServingStopAsync_ReturnsDistinctTrips()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_multi_visit");
        var stop = new Stop(stopId, "Multi Visit Station");
        context.Stops.Add(stop);

        var trip = new Trip(TripId.From("trip_multi_visit"), "Circular");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        // Create multiple StopTimes for the same trip at the same stop (e.g., circular route)
        await InsertStopTimeAsync(context, "trip_multi_visit", "stop_multi_visit", 1, 480, 480); // 8:00
        await InsertStopTimeAsync(context, "trip_multi_visit", "stop_multi_visit", 5, 540, 540); // 9:00

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetTripsServingStopAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1); // Should be distinct - only one trip
        result.Value[0].Id.Value.Should().Be("trip_multi_visit");
    }

    [Fact]
    public async Task GetTripsServingStopAsync_ReturnsReadOnlyList()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_readonly");
        var stop = new Stop(stopId, "Readonly Station");
        context.Stops.Add(stop);

        var trip = new Trip(TripId.From("trip_readonly_test"), "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        await InsertStopTimeAsync(context, "trip_readonly_test", "stop_readonly", 1, 480, 480); // 8:00

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetTripsServingStopAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IReadOnlyList<Trip>>();
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
        result.Error.Should().Contain("Trips collection cannot be null");
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var result = await repository.AddRangeAsync(Array.Empty<Trip>());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_WithValidTrips_AddsTripsToDatabase()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var trips = new[]
        {
            new Trip(TripId.From("add_trip_1"), "Hamburg Hauptbahnhof"),
            new Trip(TripId.From("add_trip_2"), "Hamburg Dammtor"),
        };

        // Act
        var result = await repository.AddRangeAsync(trips);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify trips were added
        using var verifyContext = CreateContext();
        var count = await verifyContext.Trips.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task AddRangeAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var trips = new[] { new Trip(TripId.From("trip_cancel"), "Test") };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.AddRangeAsync(trips, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AddRangeAsync_WithDuplicateIds_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var existingTrip = new Trip(TripId.From("duplicate_trip"), "Existing Trip");
        context.Trips.Add(existingTrip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);
        var newTrips = new[] { new Trip(TripId.From("duplicate_trip"), "New Trip") };

        // Act
        var result = await repository.AddRangeAsync(newTrips);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Error message should indicate failure - either database error or generic error
        result.Error.Should().ContainAny("error", "Error");
    }

    [Fact]
    public async Task AddRangeAsync_WithLargeCollection_AddsAllTrips()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var trips = Enumerable.Range(1, 100)
            .Select(i => new Trip(TripId.From($"bulk_trip_{i}"), $"Destination {i}"))
            .ToList();

        // Act
        var result = await repository.AddRangeAsync(trips);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Trips.CountAsync();
        count.Should().Be(100);
    }

    [Fact]
    public async Task AddRangeAsync_WithTripsHavingAllProperties_PersistsAllProperties()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var tripId = TripId.From("full_props_trip");
        var trips = new[]
        {
            new Trip(tripId, "Berlin Hauptbahnhof", "ICE 123")
        };

        // Act
        var result = await repository.AddRangeAsync(trips);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var loaded = await verifyContext.Trips.FirstAsync(t => t.Id == tripId);
        loaded.Headsign.Should().Be("Berlin Hauptbahnhof");
        loaded.ShortName.Should().Be("ICE 123");
    }

    [Fact]
    public async Task AddRangeAsync_WithTripsHavingNullOptionalProperties_PersistsTrips()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var tripId = TripId.From("minimal_trip");
        var trips = new[]
        {
            new Trip(tripId)
        };

        // Act
        var result = await repository.AddRangeAsync(trips);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var loaded = await verifyContext.Trips.FirstAsync(t => t.Id == tripId);
        loaded.Headsign.Should().BeNull();
        loaded.ShortName.Should().BeNull();
    }

    #endregion

    #region AsNoTracking Verification Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsUntrackedEntity()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("untracked_trip");
        var trip = new Trip(tripId, "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        context.Entry(result.Value).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetByRouteAsync_ReturnsUntrackedEntities()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_untracked");
        var trip = new Trip(TripId.From("untracked_route_trip"), "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();
        context.Entry(trip).Property("RouteId").CurrentValue = routeId.Value;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var t in result.Value)
        {
            context.Entry(t).State.Should().Be(EntityState.Detached);
        }
    }

    [Fact]
    public async Task GetTripsServingStopAsync_ReturnsUntrackedEntities()
    {
        // Arrange
        using var context = CreateContext();
        var stopId = StopId.From("stop_untracked");
        var stop = new Stop(stopId, "Untracked Station");
        context.Stops.Add(stop);

        var trip = new Trip(TripId.From("untracked_serving_trip"), "Test");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        await InsertStopTimeAsync(context, "untracked_serving_trip", "stop_untracked", 1, 480, 480); // 8:00
        context.ChangeTracker.Clear();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetTripsServingStopAsync(stopId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var t in result.Value)
        {
            context.Entry(t).State.Should().Be(EntityState.Detached);
        }
    }

    #endregion

    #region Bogus Data Tests

    [Fact]
    public async Task GetByIdAsync_WithBogusGeneratedTrip_WorksCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var tripFaker = new Faker<Trip>()
            .CustomInstantiator(f => new Trip(
                TripId.From($"bogus_trip_{f.Random.AlphaNumeric(5)}"),
                f.Address.City() + " " + f.PickRandom("Hbf", "Süd", "Nord", "West", "Ost"),
                f.PickRandom("S1", "S2", "U1", "U2", "RE1", "ICE")));

        var fakeTrip = tripFaker.Generate();
        context.Trips.Add(fakeTrip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(fakeTrip.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Headsign.Should().Be(fakeTrip.Headsign);
        result.Value.ShortName.Should().Be(fakeTrip.ShortName);
    }

    [Fact]
    public async Task AddRangeAsync_WithBogusGeneratedTrips_AddsAllTrips()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);
        var counter = 0;
        var tripFaker = new Faker<Trip>()
            .CustomInstantiator(f =>
            {
                counter++;
                return new Trip(
                    TripId.From($"bogus_bulk_{counter}"),
                    f.Address.City() + " " + f.PickRandom("Hbf", "Süd", "Nord"),
                    f.PickRandom("S1", "U1", "RE1"));
            });

        var fakeTrips = tripFaker.Generate(10);

        // Act
        var result = await repository.AddRangeAsync(fakeTrips);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using var verifyContext = CreateContext();
        var count = await verifyContext.Trips.CountAsync();
        count.Should().Be(10);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetByIdAsync_WithSpecialCharactersInHeadsign_ReturnsTrip()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("trip_special");
        var trip = new Trip(tripId, "Frankfurt (Main) Hbf → München", "ICE-Sprinter");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Headsign.Should().Be("Frankfurt (Main) Hbf → München");
        result.Value.ShortName.Should().Be("ICE-Sprinter");
    }

    [Fact]
    public async Task GetByIdAsync_WithGermanUmlautsInHeadsign_ReturnsTrip()
    {
        // Arrange
        using var context = CreateContext();
        var tripId = TripId.From("trip_umlaut");
        var trip = new Trip(tripId, "Köln Hbf → Düsseldorf", "Regionalexpreß");
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByIdAsync(tripId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Headsign.Should().Be("Köln Hbf → Düsseldorf");
        result.Value.ShortName.Should().Be("Regionalexpreß");
    }

    [Fact]
    public async Task GetByRouteAsync_WithTripsHavingNullRouteId_ExcludesThoseTrips()
    {
        // Arrange
        using var context = CreateContext();
        var routeId = RouteId.From("route_with_null");

        var tripWithRoute = new Trip(TripId.From("trip_with_route"), "With Route");
        var tripWithoutRoute = new Trip(TripId.From("trip_without_route"), "Without Route");

        context.Trips.Add(tripWithRoute);
        context.Trips.Add(tripWithoutRoute);
        await context.SaveChangesAsync();

        // Only set RouteId for the first trip
        context.Entry(tripWithRoute).Property("RouteId").CurrentValue = routeId.Value;
        // tripWithoutRoute's RouteId remains null
        await context.SaveChangesAsync();

        var repository = CreateRepository(context);

        // Act
        var result = await repository.GetByRouteAsync(routeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Value.Should().Be("trip_with_route");
    }

    #endregion
}
