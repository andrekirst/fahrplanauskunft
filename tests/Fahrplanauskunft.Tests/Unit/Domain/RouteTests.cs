using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Fahrplanauskunft.Tests.Unit.Domain;

public class RouteTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var id = RouteId.From("R1");
        var shortName = "U1";
        var mode = TransportMode.Subway;

        // Act
        var route = new Route(id, shortName, mode);

        // Assert
        route.Id.Should().Be(id);
        route.ShortName.Should().Be(shortName);
        route.Mode.Should().Be(mode);
        route.LongName.Should().BeNull();
        route.Trips.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var id = RouteId.From("R1");
        var shortName = "U1";
        var mode = TransportMode.Subway;
        var longName = "University Line 1";
        var color = "FF0000";
        var textColor = "FFFFFF";

        // Act
        var route = new Route(id, shortName, mode, longName, color, textColor);

        // Assert
        route.LongName.Should().Be(longName);
        route.Color.Should().Be(color);
        route.TextColor.Should().Be(textColor);
    }

    [Fact]
    public void Constructor_WithNullShortName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = RouteId.From("R1");

        // Act
        var act = () => new Route(id, null!, TransportMode.Bus);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyShortName_ShouldThrowArgumentException()
    {
        // Arrange
        var id = RouteId.From("R1");

        // Act
        var act = () => new Route(id, "", TransportMode.Bus);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithTrips_ShouldAddAllTrips()
    {
        // Arrange
        var trips = new[]
        {
            new Trip(TripId.From("T1")),
            new Trip(TripId.From("T2")),
            new Trip(TripId.From("T3"))
        };

        // Act
        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus, trips);

        // Assert
        route.Trips.Should().HaveCount(3);
        route.TripCount.Should().Be(3);
    }

    [Fact]
    public void Create_ShouldCreateSimpleRoute()
    {
        // Act
        var route = Route.Create("R1", "U1", TransportMode.Subway);

        // Assert
        route.Id.Should().Be(RouteId.From("R1"));
        route.ShortName.Should().Be("U1");
        route.Mode.Should().Be(TransportMode.Subway);
    }

    [Fact]
    public void AddTrip_ShouldAddTripToRoute()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);
        var trip = new Trip(TripId.From("T1"));

        // Act
        route.AddTrip(trip);

        // Assert
        route.Trips.Should().Contain(trip);
        route.TripCount.Should().Be(1);
    }

    [Fact]
    public void AddTrips_ShouldAddMultipleTrips()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);
        var trips = new[]
        {
            new Trip(TripId.From("T1")),
            new Trip(TripId.From("T2"))
        };

        // Act
        route.AddTrips(trips);

        // Assert
        route.Trips.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveTrip_ShouldRemoveTripFromRoute()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));
        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus, new[] { trip });

        // Act
        var result = route.RemoveTrip(trip);

        // Assert
        result.Should().BeTrue();
        route.Trips.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTrip_WithNonExistentTrip_ShouldReturnFalse()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);
        var trip = new Trip(TripId.From("T1"));

        // Act
        var result = route.RemoveTrip(trip);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasTrip_WithExistingTrip_ShouldReturnTrue()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"));
        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus, new[] { trip });

        // Act & Assert
        route.HasTrip(TripId.From("T1")).Should().BeTrue();
    }

    [Fact]
    public void HasTrip_WithNonExistentTrip_ShouldReturnFalse()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act & Assert
        route.HasTrip(TripId.From("T1")).Should().BeFalse();
    }

    [Fact]
    public void GetTrip_WithExistingTrip_ShouldReturnTrip()
    {
        // Arrange
        var trip = new Trip(TripId.From("T1"), headsign: "Central");
        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus, new[] { trip });

        // Act
        var result = route.GetTrip(TripId.From("T1"));

        // Assert
        result.Should().NotBeNull();
        result!.Headsign.Should().Be("Central");
    }

    [Fact]
    public void GetTrip_WithNonExistentTrip_ShouldReturnNull()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var result = route.GetTrip(TripId.From("T1"));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTripsServingStop_ShouldReturnMatchingTrips()
    {
        // Arrange
        var stop = Stop.Create("S1", "Central");
        var tripWithStop = new Trip(TripId.From("T1"));
        tripWithStop.AddStopTime(StopTime.Create(stop, StopSequence.From(1), new TimeOfDay(8, 0)));

        var tripWithoutStop = new Trip(TripId.From("T2"));

        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus,
            new[] { tripWithStop, tripWithoutStop });

        // Act
        var result = route.GetTripsServingStop(stop).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(TripId.From("T1"));
    }

    [Fact]
    public void Trips_ShouldBeReadOnly()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var trips = route.Trips;

        // Assert
        trips.Should().BeAssignableTo<IReadOnlyList<Trip>>();
    }

    [Fact]
    public void DisplayName_WithLongName_ShouldReturnLongName()
    {
        // Arrange
        var route = new Route(RouteId.From("R1"), "U1", TransportMode.Subway, longName: "University Line 1");

        // Act & Assert
        route.DisplayName.Should().Be("University Line 1");
    }

    [Fact]
    public void DisplayName_WithoutLongName_ShouldReturnShortName()
    {
        // Arrange
        var route = Route.Create("R1", "U1", TransportMode.Subway);

        // Act & Assert
        route.DisplayName.Should().Be("U1");
    }

    [Fact]
    public void RouteLabel_ShouldCombineModeAbbreviationAndShortName()
    {
        // Arrange
        var route = Route.Create("R1", "1", TransportMode.Subway);

        // Act & Assert
        route.RouteLabel.Should().Be("U1");  // U for Subway
    }

    [Fact]
    public void Equality_RoutesWithSameId_ShouldBeEqual()
    {
        // Arrange
        var route1 = new Route(RouteId.From("R1"), "42", TransportMode.Bus);
        var route2 = new Route(RouteId.From("R1"), "43", TransportMode.Tram);

        // Act & Assert
        route1.Should().Be(route2);
        (route1 == route2).Should().BeTrue();
    }

    [Fact]
    public void Equality_RoutesWithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var route1 = Route.Create("R1", "42", TransportMode.Bus);
        var route2 = Route.Create("R2", "42", TransportMode.Bus);

        // Act & Assert
        route1.Should().NotBe(route2);
        (route1 != route2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldIncludeModeAndShortName()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var result = route.ToString();

        // Assert
        result.Should().Contain("Bus");
        result.Should().Contain("42");
    }

    [Fact]
    public void ToString_WithTrips_ShouldIncludeTripCount()
    {
        // Arrange
        var trips = new[] { new Trip(TripId.From("T1")), new Trip(TripId.From("T2")) };
        var route = new Route(RouteId.From("R1"), "42", TransportMode.Bus, trips);

        // Act
        var result = route.ToString();

        // Assert
        result.Should().Contain("2 trips");
    }

    [Fact]
    public void AddTrip_WithNullTrip_ShouldThrowArgumentNullException()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var act = () => route.AddTrip(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddTrips_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var act = () => route.AddTrips(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveTrip_WithNullTrip_ShouldThrowArgumentNullException()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var act = () => route.RemoveTrip(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetTripsServingStop_WithNullStop_ShouldThrowArgumentNullException()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act
        var act = () => route.GetTripsServingStop(null!).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullTripsCollection_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new Route(RouteId.From("R1"), "42", TransportMode.Bus, (IEnumerable<Trip>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equality_WithNull_ShouldNotBeEqual()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act & Assert
        route.Equals(null).Should().BeFalse();
        (route == null).Should().BeFalse();
        (null == route).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithSameReference_ShouldBeEqual()
    {
        // Arrange
        var route = Route.Create("R1", "42", TransportMode.Bus);

        // Act & Assert
        route.Equals(route).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (route == route).Should().BeTrue();
#pragma warning restore CS1718
    }

    [Fact]
    public void GetHashCode_ForEqualRoutes_ShouldBeSame()
    {
        // Arrange
        var route1 = Route.Create("R1", "42", TransportMode.Bus);
        var route2 = Route.Create("R1", "43", TransportMode.Tram);

        // Act & Assert
        route1.GetHashCode().Should().Be(route2.GetHashCode());
    }

    [Fact]
    public void Constructor_WithWhitespaceShortName_ShouldThrowArgumentException()
    {
        // Arrange
        var id = RouteId.From("R1");

        // Act
        var act = () => new Route(id, "   ", TransportMode.Bus);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
