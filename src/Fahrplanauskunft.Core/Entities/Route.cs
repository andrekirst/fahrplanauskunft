using Ardalis.GuardClauses;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Entities;

/// <summary>
/// Represents a transit route - a named service that operates along a path.
/// A route contains multiple trips (specific journeys at different times).
/// Examples: "U1", "Bus 42", "S-Bahn S1".
/// </summary>
public sealed class Route : IEquatable<Route>
{
    private readonly List<Trip> _trips;

    /// <summary>
    /// The unique identifier of this route.
    /// </summary>
    public RouteId Id { get; }

    /// <summary>
    /// The short name of the route (e.g., "U1", "42", "S1").
    /// </summary>
    public string ShortName { get; }

    /// <summary>
    /// The long/full name of the route (e.g., "University Line 1").
    /// </summary>
    public string? LongName { get; }

    /// <summary>
    /// The transport mode of this route (e.g., Bus, Subway, Tram).
    /// </summary>
    public TransportMode Mode { get; }

    /// <summary>
    /// Optional color code for the route (hex format, e.g., "FF0000").
    /// </summary>
    public string? Color { get; }

    /// <summary>
    /// Optional text color for the route (hex format, e.g., "FFFFFF").
    /// </summary>
    public string? TextColor { get; }

    /// <summary>
    /// The trips that operate on this route.
    /// </summary>
    public IReadOnlyList<Trip> Trips => _trips.AsReadOnly();

    /// <summary>
    /// The number of trips on this route.
    /// </summary>
    public int TripCount => _trips.Count;

    /// <summary>
    /// Creates a new Route.
    /// </summary>
    /// <param name="id">The route identifier</param>
    /// <param name="shortName">The short name (cannot be null or whitespace)</param>
    /// <param name="mode">The transport mode</param>
    /// <param name="longName">Optional long name</param>
    /// <param name="color">Optional route color in hex format</param>
    /// <param name="textColor">Optional text color in hex format</param>
    /// <exception cref="ArgumentNullException">Thrown when shortName is null</exception>
    /// <exception cref="ArgumentException">Thrown when shortName is empty or whitespace</exception>
    public Route(
        RouteId id,
        string shortName,
        TransportMode mode,
        string? longName = null,
        string? color = null,
        string? textColor = null)
    {
        Guard.Against.NullOrWhiteSpace(shortName, nameof(shortName), "Route short name cannot be null or empty.");

        Id = id;
        ShortName = shortName;
        Mode = mode;
        LongName = longName;
        Color = color;
        TextColor = textColor;
        _trips = new List<Trip>();
    }

    /// <summary>
    /// Creates a new Route with the specified trips.
    /// </summary>
    /// <param name="id">The route identifier</param>
    /// <param name="shortName">The short name</param>
    /// <param name="mode">The transport mode</param>
    /// <param name="trips">The trips to add</param>
    /// <param name="longName">Optional long name</param>
    /// <param name="color">Optional route color</param>
    /// <param name="textColor">Optional text color</param>
    public Route(
        RouteId id,
        string shortName,
        TransportMode mode,
        IEnumerable<Trip> trips,
        string? longName = null,
        string? color = null,
        string? textColor = null)
        : this(id, shortName, mode, longName, color, textColor)
    {
        Guard.Against.Null(trips, nameof(trips));

        foreach (var trip in trips)
        {
            Guard.Against.Null(trip, nameof(trip));
            _trips.Add(trip);
        }
    }

    /// <summary>
    /// Creates a simple route with just ID, name, and mode.
    /// </summary>
    /// <param name="id">The route ID as a string</param>
    /// <param name="shortName">The short name</param>
    /// <param name="mode">The transport mode</param>
    /// <returns>A new Route instance</returns>
    public static Route Create(string id, string shortName, TransportMode mode)
    {
        return new Route(RouteId.From(id), shortName, mode);
    }

    /// <summary>
    /// Adds a trip to this route.
    /// </summary>
    /// <param name="trip">The trip to add</param>
    /// <exception cref="ArgumentNullException">Thrown when trip is null</exception>
    public void AddTrip(Trip trip)
    {
        Guard.Against.Null(trip, nameof(trip));
        _trips.Add(trip);
    }

    /// <summary>
    /// Adds multiple trips to this route.
    /// </summary>
    /// <param name="trips">The trips to add</param>
    /// <exception cref="ArgumentNullException">Thrown when trips is null or contains null</exception>
    public void AddTrips(IEnumerable<Trip> trips)
    {
        Guard.Against.Null(trips, nameof(trips));

        foreach (var trip in trips)
        {
            Guard.Against.Null(trip, nameof(trip));
            _trips.Add(trip);
        }
    }

    /// <summary>
    /// Removes a trip from this route.
    /// </summary>
    /// <param name="trip">The trip to remove</param>
    /// <returns>True if the trip was removed</returns>
    public bool RemoveTrip(Trip trip)
    {
        Guard.Against.Null(trip, nameof(trip));
        return _trips.Remove(trip);
    }

    /// <summary>
    /// Checks if this route has a trip with the specified ID.
    /// </summary>
    /// <param name="tripId">The trip ID to check</param>
    /// <returns>True if a trip with that ID exists</returns>
    public bool HasTrip(TripId tripId)
    {
        return _trips.Any(t => t.Id == tripId);
    }

    /// <summary>
    /// Gets a trip by its ID.
    /// </summary>
    /// <param name="tripId">The trip ID</param>
    /// <returns>The trip, or null if not found</returns>
    public Trip? GetTrip(TripId tripId)
    {
        return _trips.Find(t => t.Id == tripId);
    }

    /// <summary>
    /// Gets all trips that serve a specific stop.
    /// </summary>
    /// <param name="stop">The stop to filter by</param>
    /// <returns>Trips that serve the stop</returns>
    public IEnumerable<Trip> GetTripsServingStop(Stop stop)
    {
        Guard.Against.Null(stop, nameof(stop));
        return _trips.Where(t => t.ServesStop(stop));
    }

    /// <summary>
    /// Gets the display name for this route.
    /// Returns the long name if available, otherwise the short name.
    /// </summary>
    public string DisplayName => LongName ?? ShortName;

    /// <summary>
    /// Gets the formatted route label (mode abbreviation + short name).
    /// </summary>
    public string RouteLabel => $"{Mode.GetAbbreviation()}{ShortName}";

    public bool Equals(Route? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Route other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Route? left, Route? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Route? left, Route? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        var modeName = Mode.GetDisplayName();
        var tripsPart = TripCount > 0 ? $" ({TripCount} trips)" : "";
        return $"{modeName} {ShortName}{tripsPart}";
    }
}
