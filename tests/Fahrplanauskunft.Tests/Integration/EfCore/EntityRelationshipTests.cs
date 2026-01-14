using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fahrplanauskunft.Tests.Integration.EfCore;

/// <summary>
/// Integration tests for entity relationships and materialization.
/// Tests the full round-trip of creating complex entity graphs and loading them back.
/// </summary>
public class EntityRelationshipTests : IClassFixture<SqliteDbContextFixture>
{
    private readonly SqliteDbContextFixture _fixture;

    public EntityRelationshipTests(SqliteDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region Route-Trip Relationship Tests

    [Fact]
    public async Task Route_CanHaveMultipleTrips()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var routeId = RouteId.From("route_multi_trips");
        var route = new Route(routeId, "S42", TransportMode.Rail, "Ringbahn");

        var trip1 = new Trip(TripId.From("trip_ring_1"), "Ringbahn -> Gesundbrunnen");
        var trip2 = new Trip(TripId.From("trip_ring_2"), "Ringbahn -> Westkreuz");

        context.Routes.Add(route);
        await context.SaveChangesAsync();

        // Set up relationship via shadow property
        context.Trips.Add(trip1);
        context.Entry(trip1).Property("RouteId").CurrentValue = routeId.Value;

        context.Trips.Add(trip2);
        context.Entry(trip2).Property("RouteId").CurrentValue = routeId.Value;

        await context.SaveChangesAsync();

        // Assert - Load trips for route
        using var verifyContext = _fixture.CreateContext();
        var trips = await verifyContext.Trips
            .Where(t => EF.Property<string>(t, "RouteId") == routeId.Value)
            .ToListAsync();

        trips.Should().HaveCount(2);
        trips.Should().Contain(t => t.Headsign == "Ringbahn -> Gesundbrunnen");
        trips.Should().Contain(t => t.Headsign == "Ringbahn -> Westkreuz");
    }

    #endregion

    #region Trip-StopTime Relationship Tests

    [Fact]
    public async Task Trip_CanHaveMultipleStopTimes()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var stop1 = new Stop(StopId.From("rel_stop_1"), "First Station");
        var stop2 = new Stop(StopId.From("rel_stop_2"), "Second Station");
        var stop3 = new Stop(StopId.From("rel_stop_3"), "Third Station");

        var trip = new Trip(TripId.From("trip_multi_stops"), "Final Station");

        context.Stops.AddRange(stop1, stop2, stop3);
        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        var stopTime1 = new StopTime(stop1, StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(480), TimeOfDay.FromTotalMinutes(480));
        var stopTime2 = new StopTime(stop2, StopSequence.From(2),
            TimeOfDay.FromTotalMinutes(490), TimeOfDay.FromTotalMinutes(492));
        var stopTime3 = new StopTime(stop3, StopSequence.From(3),
            TimeOfDay.FromTotalMinutes(505), TimeOfDay.FromTotalMinutes(505));

        // Set shadow properties via Entry BEFORE marking as Added
        var entry1 = context.Entry(stopTime1);
        entry1.Property("TripId").CurrentValue = trip.Id.Value;
        entry1.Property("StopId").CurrentValue = stop1.Id.Value;
        entry1.State = EntityState.Added;

        var entry2 = context.Entry(stopTime2);
        entry2.Property("TripId").CurrentValue = trip.Id.Value;
        entry2.Property("StopId").CurrentValue = stop2.Id.Value;
        entry2.State = EntityState.Added;

        var entry3 = context.Entry(stopTime3);
        entry3.Property("TripId").CurrentValue = trip.Id.Value;
        entry3.Property("StopId").CurrentValue = stop3.Id.Value;
        entry3.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var stopTimes = await verifyContext.StopTimes
            .Where(st => EF.Property<string>(st, "TripId") == trip.Id.Value)
            .ToListAsync();

        // Order client-side since Sequence.Value can't be translated
        var orderedStopTimes = stopTimes.OrderBy(st => st.Sequence.Value).ToList();

        orderedStopTimes.Should().HaveCount(3);

        // Verify stop IDs via shadow properties
        var stopIds = orderedStopTimes.Select(st =>
            verifyContext.Entry(st).Property("StopId").CurrentValue as string).ToList();

        stopIds[0].Should().Be(stop1.Id.Value);
        stopIds[1].Should().Be(stop2.Id.Value);
        stopIds[2].Should().Be(stop3.Id.Value);
    }

    #endregion

    #region Stop-StopTime Relationship Tests

    [Fact]
    public async Task Stop_CanBeReferencedByMultipleStopTimes()
    {
        // Arrange - A central station referenced by multiple trips
        using var context = _fixture.CreateContext();

        var centralStation = new Stop(StopId.From("central_hub"), "Central Hub");

        var trip1 = new Trip(TripId.From("trip_through_hub_1"), "North");
        var trip2 = new Trip(TripId.From("trip_through_hub_2"), "South");

        context.Stops.Add(centralStation);
        context.Trips.AddRange(trip1, trip2);
        await context.SaveChangesAsync();

        var stopTime1 = new StopTime(centralStation, StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(600), TimeOfDay.FromTotalMinutes(605));
        var stopTime2 = new StopTime(centralStation, StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(630), TimeOfDay.FromTotalMinutes(635));

        // Set shadow properties via Entry BEFORE marking as Added
        var entry1 = context.Entry(stopTime1);
        entry1.Property("TripId").CurrentValue = trip1.Id.Value;
        entry1.Property("StopId").CurrentValue = centralStation.Id.Value;
        entry1.State = EntityState.Added;

        var entry2 = context.Entry(stopTime2);
        entry2.Property("TripId").CurrentValue = trip2.Id.Value;
        entry2.Property("StopId").CurrentValue = centralStation.Id.Value;
        entry2.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Assert - Query stop times at central hub
        using var verifyContext = _fixture.CreateContext();
        var hubStopTimes = await verifyContext.StopTimes
            .Where(st => EF.Property<string>(st, "StopId") == centralStation.Id.Value)
            .ToListAsync();

        hubStopTimes.Should().HaveCount(2);
    }

    #endregion

    #region Footpath Bidirectional Tests

    [Fact]
    public async Task Footpath_BidirectionalWalking_CanBeRepresented()
    {
        // Arrange - Walking between two nearby stations
        using var context = _fixture.CreateContext();

        var stationA = new Stop(StopId.From("walk_a"), "Station A");
        var stationB = new Stop(StopId.From("walk_b"), "Station B");

        context.Stops.AddRange(stationA, stationB);
        await context.SaveChangesAsync();

        // Create footpath in both directions
        var footpathAtoB = new Footpath(stationA, stationB, Duration.FromMinutes(5));
        var footpathBtoA = new Footpath(stationB, stationA, Duration.FromMinutes(5));

        // Set shadow properties via Entry BEFORE marking as Added
        var entryAtoB = context.Entry(footpathAtoB);
        entryAtoB.Property("FromStopId").CurrentValue = stationA.Id.Value;
        entryAtoB.Property("ToStopId").CurrentValue = stationB.Id.Value;
        entryAtoB.State = EntityState.Added;

        var entryBtoA = context.Entry(footpathBtoA);
        entryBtoA.Property("FromStopId").CurrentValue = stationB.Id.Value;
        entryBtoA.Property("ToStopId").CurrentValue = stationA.Id.Value;
        entryBtoA.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var footpaths = await verifyContext.Footpaths
            .Where(f =>
                (EF.Property<string>(f, "FromStopId") == stationA.Id.Value &&
                 EF.Property<string>(f, "ToStopId") == stationB.Id.Value) ||
                (EF.Property<string>(f, "FromStopId") == stationB.Id.Value &&
                 EF.Property<string>(f, "ToStopId") == stationA.Id.Value))
            .ToListAsync();

        footpaths.Should().HaveCount(2);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public async Task CanQuery_StopTimesByArrivalTime()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var stop = new Stop(StopId.From("query_arr_stop"), "Query Arrival Stop");
        var trip1 = new Trip(TripId.From("early_trip"));
        var trip2 = new Trip(TripId.From("late_trip"));

        context.Stops.Add(stop);
        context.Trips.AddRange(trip1, trip2);
        await context.SaveChangesAsync();

        var earlyStopTime = new StopTime(stop, StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(360), TimeOfDay.FromTotalMinutes(362)); // 6:00
        var lateStopTime = new StopTime(stop, StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(720), TimeOfDay.FromTotalMinutes(722)); // 12:00

        // Set shadow properties via Entry BEFORE marking as Added
        var entryEarly = context.Entry(earlyStopTime);
        entryEarly.Property("TripId").CurrentValue = trip1.Id.Value;
        entryEarly.Property("StopId").CurrentValue = stop.Id.Value;
        entryEarly.State = EntityState.Added;

        var entryLate = context.Entry(lateStopTime);
        entryLate.Property("TripId").CurrentValue = trip2.Id.Value;
        entryLate.Property("StopId").CurrentValue = stop.Id.Value;
        entryLate.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Act - Find departures after 10:00 (600 minutes)
        using var queryContext = _fixture.CreateContext();
        var allStopTimes = await queryContext.StopTimes
            .Where(st => EF.Property<string>(st, "StopId") == stop.Id.Value)
            .ToListAsync();

        // Filter client-side since Arrival.TotalMinutes can't be translated
        var laterDepartures = allStopTimes.Where(st => st.Arrival.TotalMinutes >= 600).ToList();

        // Assert
        laterDepartures.Should().HaveCount(1);
        laterDepartures[0].Arrival.TotalMinutes.Should().Be(720);
    }

    [Fact]
    public async Task CanQuery_RoutesByTransportMode()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var busRoute = new Route(RouteId.From("bus_query"), "100", TransportMode.Bus);
        var tramRoute = new Route(RouteId.From("tram_query"), "M1", TransportMode.Tram);

        context.Routes.AddRange(busRoute, tramRoute);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = _fixture.CreateContext();
        var busRoutes = await queryContext.Routes
            .Where(r => r.Mode == TransportMode.Bus)
            .ToListAsync();

        var tramRoutes = await queryContext.Routes
            .Where(r => r.Mode == TransportMode.Tram)
            .ToListAsync();

        // Assert
        busRoutes.Should().Contain(r => r.ShortName == "100");
        tramRoutes.Should().Contain(r => r.ShortName == "M1");
    }

    [Fact]
    public async Task CanQuery_WheelchairAccessibleFootpaths()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var stop1 = new Stop(StopId.From("access_stop_1"), "Accessible Station 1");
        var stop2 = new Stop(StopId.From("access_stop_2"), "Accessible Station 2");
        var stop3 = new Stop(StopId.From("no_access_stop"), "No Access Station");

        context.Stops.AddRange(stop1, stop2, stop3);
        await context.SaveChangesAsync();

        var accessiblePath = new Footpath(stop1, stop2, Duration.FromMinutes(3), true);
        var nonAccessiblePath = new Footpath(stop1, stop3, Duration.FromMinutes(2), false);

        // Set shadow properties via Entry BEFORE marking as Added
        var entryAccessible = context.Entry(accessiblePath);
        entryAccessible.Property("FromStopId").CurrentValue = stop1.Id.Value;
        entryAccessible.Property("ToStopId").CurrentValue = stop2.Id.Value;
        entryAccessible.State = EntityState.Added;

        var entryNonAccessible = context.Entry(nonAccessiblePath);
        entryNonAccessible.Property("FromStopId").CurrentValue = stop1.Id.Value;
        entryNonAccessible.Property("ToStopId").CurrentValue = stop3.Id.Value;
        entryNonAccessible.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Act
        using var queryContext = _fixture.CreateContext();
        var accessiblePaths = await queryContext.Footpaths
            .Where(f => f.WheelchairAccessible)
            .Where(f => EF.Property<string>(f, "FromStopId") == stop1.Id.Value)
            .ToListAsync();

        // Assert
        accessiblePaths.Should().HaveCount(1);
        var accessibleEntry = queryContext.Entry(accessiblePaths[0]);
        accessibleEntry.Property("ToStopId").CurrentValue.Should().Be(stop2.Id.Value);
    }

    #endregion

    #region Full Transit Schedule Test

    [Fact]
    public async Task CanCreate_CompleteTransitSchedule()
    {
        // Arrange - Create a complete mini transit system
        using var context = _fixture.CreateContext();

        // Stops
        var hauptbahnhof = new Stop(
            StopId.From("hbf"),
            "Hauptbahnhof",
            new Coordinates(52.525, 13.369),
            "Platform 1",
            true);

        var alexanderplatz = new Stop(
            StopId.From("alex"),
            "Alexanderplatz",
            new Coordinates(52.521, 13.411),
            wheelchairAccessible: true);

        var friedrichstrasse = new Stop(
            StopId.From("friedrichstr"),
            "Friedrichstraße",
            new Coordinates(52.520, 13.386));

        context.Stops.AddRange(hauptbahnhof, alexanderplatz, friedrichstrasse);

        // Route
        var sbahnRoute = new Route(
            RouteId.From("s5_route"),
            "S5",
            TransportMode.Rail,
            "S-Bahn S5",
            "FF6600",
            "FFFFFF");

        context.Routes.Add(sbahnRoute);

        // Trip
        var morningTrip = new Trip(
            TripId.From("s5_morning"),
            "Alexanderplatz",
            "S5");

        context.Trips.Add(morningTrip);
        await context.SaveChangesAsync();

        // Set trip route
        context.Entry(morningTrip).Property("RouteId").CurrentValue = sbahnRoute.Id.Value;

        // StopTimes
        var stopTime1 = new StopTime(
            hauptbahnhof,
            StopSequence.From(1),
            TimeOfDay.FromTotalMinutes(480),
            TimeOfDay.FromTotalMinutes(480));

        var stopTime2 = new StopTime(
            friedrichstrasse,
            StopSequence.From(2),
            TimeOfDay.FromTotalMinutes(485),
            TimeOfDay.FromTotalMinutes(486));

        var stopTime3 = new StopTime(
            alexanderplatz,
            StopSequence.From(3),
            TimeOfDay.FromTotalMinutes(492),
            TimeOfDay.FromTotalMinutes(492));

        // Set shadow properties via Entry BEFORE marking as Added
        var entry1 = context.Entry(stopTime1);
        entry1.Property("TripId").CurrentValue = morningTrip.Id.Value;
        entry1.Property("StopId").CurrentValue = hauptbahnhof.Id.Value;
        entry1.State = EntityState.Added;

        var entry2 = context.Entry(stopTime2);
        entry2.Property("TripId").CurrentValue = morningTrip.Id.Value;
        entry2.Property("StopId").CurrentValue = friedrichstrasse.Id.Value;
        entry2.State = EntityState.Added;

        var entry3 = context.Entry(stopTime3);
        entry3.Property("TripId").CurrentValue = morningTrip.Id.Value;
        entry3.Property("StopId").CurrentValue = alexanderplatz.Id.Value;
        entry3.State = EntityState.Added;

        // Footpath between stations
        var walkPath = new Footpath(
            hauptbahnhof,
            friedrichstrasse,
            Duration.FromMinutes(15),
            true,
            1200);

        var entryWalk = context.Entry(walkPath);
        entryWalk.Property("FromStopId").CurrentValue = hauptbahnhof.Id.Value;
        entryWalk.Property("ToStopId").CurrentValue = friedrichstrasse.Id.Value;
        entryWalk.State = EntityState.Added;

        await context.SaveChangesAsync();

        // Assert - Verify complete schedule can be loaded
        using var verifyContext = _fixture.CreateContext();

        // Verify stops
        var stops = await verifyContext.Stops.ToListAsync();
        stops.Should().HaveCountGreaterThanOrEqualTo(3);

        // Verify route
        var route = await verifyContext.Routes.FirstOrDefaultAsync(r => r.Id == sbahnRoute.Id);
        route.Should().NotBeNull();
        route!.Mode.Should().Be(TransportMode.Rail);

        // Verify trip
        var trip = await verifyContext.Trips.FirstOrDefaultAsync(t => t.Id == morningTrip.Id);
        trip.Should().NotBeNull();
        trip!.Headsign.Should().Be("Alexanderplatz");

        // Verify stop times for trip
        var tripStopTimesUnordered = await verifyContext.StopTimes
            .Where(st => EF.Property<string>(st, "TripId") == morningTrip.Id.Value)
            .ToListAsync();

        // Order client-side since Sequence.Value can't be translated
        var tripStopTimes = tripStopTimesUnordered.OrderBy(st => st.Sequence.Value).ToList();

        tripStopTimes.Should().HaveCount(3);

        // Verify stop IDs via shadow properties
        var stopIds = tripStopTimes.Select(st =>
            verifyContext.Entry(st).Property("StopId").CurrentValue as string).ToList();

        stopIds[0].Should().Be(hauptbahnhof.Id.Value);
        stopIds[2].Should().Be(alexanderplatz.Id.Value);

        // Verify footpath
        var footpath = await verifyContext.Footpaths
            .FirstOrDefaultAsync(f =>
                EF.Property<string>(f, "FromStopId") == hauptbahnhof.Id.Value &&
                EF.Property<string>(f, "ToStopId") == friedrichstrasse.Id.Value);

        footpath.Should().NotBeNull();
        footpath!.Duration.TotalMinutes.Should().Be(15);
        footpath.WheelchairAccessible.Should().BeTrue();
    }

    #endregion
}
