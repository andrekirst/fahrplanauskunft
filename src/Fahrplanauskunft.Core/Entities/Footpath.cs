using Ardalis.GuardClauses;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Entities;

/// <summary>
/// Represents a walking connection (transfer) between two stops.
/// Used for modeling transfers that require passengers to walk between stops.
/// </summary>
public sealed class Footpath : IEquatable<Footpath>
{
    /// <summary>
    /// The origin stop (where the walking starts).
    /// </summary>
    public Stop From { get; }

    /// <summary>
    /// The destination stop (where the walking ends).
    /// </summary>
    public Stop To { get; }

    /// <summary>
    /// The walking duration between the stops.
    /// </summary>
    public Duration Duration { get; }

    /// <summary>
    /// Indicates if this footpath is wheelchair accessible.
    /// </summary>
    public bool WheelchairAccessible { get; }

    /// <summary>
    /// The approximate distance in meters (if known).
    /// </summary>
    public int? DistanceMeters { get; }

    /// <summary>
    /// Creates a new Footpath between two stops.
    /// </summary>
    /// <param name="from">The origin stop (cannot be null)</param>
    /// <param name="to">The destination stop (cannot be null, must be different from origin)</param>
    /// <param name="duration">The walking duration</param>
    /// <param name="wheelchairAccessible">Whether the path is wheelchair accessible</param>
    /// <param name="distanceMeters">Optional distance in meters</param>
    /// <exception cref="ArgumentNullException">Thrown when from or to is null</exception>
    /// <exception cref="SelfLoopException">Thrown when from and to are the same stop</exception>
    public Footpath(
        Stop from,
        Stop to,
        Duration duration,
        bool wheelchairAccessible = false,
        int? distanceMeters = null)
    {
        Guard.Against.Null(from, nameof(from), "Origin stop cannot be null.");
        Guard.Against.Null(to, nameof(to), "Destination stop cannot be null.");

        // Validate no self-loop: origin and destination must be different
        if (from.Equals(to))
        {
            throw new SelfLoopException();
        }

        From = from;
        To = to;
        Duration = duration;
        WheelchairAccessible = wheelchairAccessible;
        DistanceMeters = distanceMeters;
    }

    /// <summary>
    /// Creates a footpath between two stops with a specified walking time in minutes.
    /// </summary>
    /// <param name="from">The origin stop</param>
    /// <param name="to">The destination stop</param>
    /// <param name="walkingMinutes">Walking time in minutes</param>
    /// <returns>A new Footpath instance</returns>
    public static Footpath Create(Stop from, Stop to, int walkingMinutes)
    {
        return new Footpath(from, to, Duration.FromMinutes(walkingMinutes));
    }

    /// <summary>
    /// Creates a footpath between two stops, estimating walking time from distance.
    /// Assumes a walking speed of 5 km/h.
    /// </summary>
    /// <param name="from">The origin stop</param>
    /// <param name="to">The destination stop</param>
    /// <param name="distanceMeters">Distance in meters</param>
    /// <returns>A new Footpath instance</returns>
    public static Footpath CreateFromDistance(Stop from, Stop to, int distanceMeters)
    {
        // Walking speed: 5 km/h = 83.33 m/min
        // Walking time = distance / speed
        const double walkingSpeedMetersPerMinute = 5000.0 / 60.0;
        var walkingMinutes = (int)Math.Ceiling(distanceMeters / walkingSpeedMetersPerMinute);

        return new Footpath(
            from,
            to,
            Duration.FromMinutes(walkingMinutes),
            wheelchairAccessible: false,
            distanceMeters: distanceMeters);
    }

    /// <summary>
    /// Creates a bidirectional pair of footpaths (from A to B and from B to A).
    /// </summary>
    /// <param name="stopA">First stop</param>
    /// <param name="stopB">Second stop</param>
    /// <param name="duration">Walking duration (same in both directions)</param>
    /// <param name="wheelchairAccessible">Accessibility status</param>
    /// <returns>A tuple containing both footpaths</returns>
    public static (Footpath Forward, Footpath Backward) CreateBidirectional(
        Stop stopA,
        Stop stopB,
        Duration duration,
        bool wheelchairAccessible = false)
    {
        return (
            new Footpath(stopA, stopB, duration, wheelchairAccessible),
            new Footpath(stopB, stopA, duration, wheelchairAccessible)
        );
    }

    /// <summary>
    /// Creates the reverse footpath (swapping origin and destination).
    /// </summary>
    /// <returns>A new Footpath in the opposite direction</returns>
    public Footpath Reverse()
    {
        return new Footpath(To, From, Duration, WheelchairAccessible, DistanceMeters);
    }

    /// <summary>
    /// Calculates the straight-line distance between the stops using coordinates.
    /// </summary>
    /// <returns>Distance in meters, or null if either stop lacks coordinates</returns>
    public double? CalculatedDistanceMeters()
    {
        if (From.Location is null || To.Location is null)
            return null;

        return From.Location.Value.DistanceToMeters(To.Location.Value);
    }

    /// <summary>
    /// Estimates if the walking duration is reasonable based on the distance.
    /// Returns true if duration is between 50% and 200% of expected walking time.
    /// </summary>
    /// <returns>True if duration seems reasonable, null if cannot be determined</returns>
    public bool? IsDurationReasonable()
    {
        var calculatedDistance = CalculatedDistanceMeters();
        if (calculatedDistance is null)
            return null;

        // Expected walking time at 5 km/h
        const double walkingSpeedMetersPerMinute = 5000.0 / 60.0;
        var expectedMinutes = calculatedDistance.Value / walkingSpeedMetersPerMinute;

        var actualMinutes = Duration.TotalMinutes;

        // Allow 50% to 200% of expected time
        return actualMinutes >= expectedMinutes * 0.5 && actualMinutes <= expectedMinutes * 2.0;
    }

    public bool Equals(Footpath? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return From.Equals(other.From) &&
               To.Equals(other.To) &&
               Duration == other.Duration;
    }

    public override bool Equals(object? obj)
    {
        return obj is Footpath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To, Duration);
    }

    public static bool operator ==(Footpath? left, Footpath? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Footpath? left, Footpath? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        var distancePart = DistanceMeters.HasValue ? $" ({DistanceMeters}m)" : "";
        return $"{From.Name} → {To.Name}: {Duration}{distancePart}";
    }
}
