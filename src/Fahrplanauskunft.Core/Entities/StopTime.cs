using Ardalis.GuardClauses;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Entities;

/// <summary>
/// Represents a scheduled arrival and departure time at a stop within a trip.
/// Encapsulates the stop, sequence, and timing information for a single stop in a journey.
/// </summary>
public sealed class StopTime : IEquatable<StopTime>, IComparable<StopTime>
{
    /// <summary>
    /// The stop where this stop time occurs.
    /// </summary>
    public Stop Stop { get; }

    /// <summary>
    /// The sequence number of this stop within the trip (1-based).
    /// </summary>
    public StopSequence Sequence { get; }

    /// <summary>
    /// The scheduled arrival time at this stop.
    /// </summary>
    public TimeOfDay Arrival { get; }

    /// <summary>
    /// The scheduled departure time from this stop.
    /// </summary>
    public TimeOfDay Departure { get; }

    /// <summary>
    /// The dwell time at this stop (time from arrival to departure).
    /// </summary>
    public Duration DwellTime => Arrival.DurationUntil(Departure);

    /// <summary>
    /// Indicates whether passengers can board at this stop.
    /// </summary>
    public bool PickupAllowed { get; }

    /// <summary>
    /// Indicates whether passengers can alight at this stop.
    /// </summary>
    public bool DropOffAllowed { get; }

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// Navigation properties are set via reflection after instantiation.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable property must contain a non-null value when exiting constructor
    private StopTime()
    {
        // Required for EF Core entity materialization
    }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new StopTime.
    /// </summary>
    /// <param name="stop">The stop (cannot be null)</param>
    /// <param name="sequence">The sequence number in the trip</param>
    /// <param name="arrival">The arrival time</param>
    /// <param name="departure">The departure time (must be >= arrival)</param>
    /// <param name="pickupAllowed">Whether boarding is allowed</param>
    /// <param name="dropOffAllowed">Whether alighting is allowed</param>
    /// <exception cref="ArgumentNullException">Thrown when stop is null</exception>
    /// <exception cref="InvalidStopTimeException">Thrown when departure is before arrival</exception>
    public StopTime(
        Stop stop,
        StopSequence sequence,
        TimeOfDay arrival,
        TimeOfDay departure,
        bool pickupAllowed = true,
        bool dropOffAllowed = true)
    {
        Guard.Against.Null(stop, nameof(stop), "Stop cannot be null.");

        // Validate that arrival is not after departure (arrival <= departure)
        if (arrival > departure)
        {
            throw new InvalidStopTimeException(
                $"Arrival time ({arrival}) cannot be after departure time ({departure}) at stop {stop.Name}.");
        }

        Stop = stop;
        Sequence = sequence;
        Arrival = arrival;
        Departure = departure;
        PickupAllowed = pickupAllowed;
        DropOffAllowed = dropOffAllowed;
    }

    /// <summary>
    /// Creates a StopTime where arrival and departure are the same (no dwell time).
    /// </summary>
    /// <param name="stop">The stop</param>
    /// <param name="sequence">The sequence number</param>
    /// <param name="time">The arrival and departure time</param>
    /// <returns>A new StopTime instance</returns>
    public static StopTime Create(Stop stop, StopSequence sequence, TimeOfDay time)
    {
        return new StopTime(stop, sequence, time, time);
    }

    /// <summary>
    /// Creates a StopTime with specified arrival and departure times.
    /// </summary>
    /// <param name="stop">The stop</param>
    /// <param name="sequence">The sequence number</param>
    /// <param name="arrival">The arrival time</param>
    /// <param name="departure">The departure time</param>
    /// <returns>A new StopTime instance</returns>
    public static StopTime Create(Stop stop, StopSequence sequence, TimeOfDay arrival, TimeOfDay departure)
    {
        return new StopTime(stop, sequence, arrival, departure);
    }

    /// <summary>
    /// Creates a StopTime that is arrival-only (no pickup allowed).
    /// Typically used for the last stop of a trip.
    /// </summary>
    /// <param name="stop">The stop</param>
    /// <param name="sequence">The sequence number</param>
    /// <param name="arrival">The arrival time</param>
    /// <returns>A new StopTime instance</returns>
    public static StopTime CreateArrivalOnly(Stop stop, StopSequence sequence, TimeOfDay arrival)
    {
        return new StopTime(stop, sequence, arrival, arrival, pickupAllowed: false, dropOffAllowed: true);
    }

    /// <summary>
    /// Creates a StopTime that is departure-only (no drop-off allowed).
    /// Typically used for the first stop of a trip.
    /// </summary>
    /// <param name="stop">The stop</param>
    /// <param name="sequence">The sequence number</param>
    /// <param name="departure">The departure time</param>
    /// <returns>A new StopTime instance</returns>
    public static StopTime CreateDepartureOnly(Stop stop, StopSequence sequence, TimeOfDay departure)
    {
        return new StopTime(stop, sequence, departure, departure, pickupAllowed: true, dropOffAllowed: false);
    }

    /// <summary>
    /// Compares stop times by their sequence number.
    /// </summary>
    public int CompareTo(StopTime? other)
    {
        if (other is null) return 1;
        return Sequence.CompareTo(other.Sequence);
    }

    public bool Equals(StopTime? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Stop.Equals(other.Stop) &&
               Sequence == other.Sequence &&
               Arrival == other.Arrival &&
               Departure == other.Departure;
    }

    public override bool Equals(object? obj)
    {
        return obj is StopTime other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Stop, Sequence, Arrival, Departure);
    }

    public static bool operator ==(StopTime? left, StopTime? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(StopTime? left, StopTime? right)
    {
        return !(left == right);
    }

    public static bool operator <(StopTime? left, StopTime? right)
    {
        if (left is null) return right is not null;
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(StopTime? left, StopTime? right)
    {
        if (left is null) return false;
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(StopTime? left, StopTime? right)
    {
        if (left is null) return true;
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(StopTime? left, StopTime? right)
    {
        if (left is null) return right is null;
        return left.CompareTo(right) >= 0;
    }

    public override string ToString()
    {
        return Arrival == Departure
            ? $"#{Sequence}: {Stop.Name} @ {Arrival}"
            : $"#{Sequence}: {Stop.Name} arr {Arrival} dep {Departure}";
    }
}
