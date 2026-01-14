using Ardalis.GuardClauses;
using Fahrplanauskunft.Core.Exceptions;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Entities;

/// <summary>
/// Represents a trip - a specific journey of a vehicle along a route.
/// A trip consists of an ordered sequence of stop times, where each stop has a unique sequence number.
/// </summary>
public sealed class Trip : IEquatable<Trip>
{
    private readonly List<StopTime> _stopTimes;
    private readonly HashSet<int> _usedSequenceNumbers;

    /// <summary>
    /// The unique identifier of this trip.
    /// </summary>
    public TripId Id { get; }

    /// <summary>
    /// Optional headsign displayed on the vehicle (e.g., "Central Station").
    /// </summary>
    public string? Headsign { get; }

    /// <summary>
    /// Optional short name for the trip (e.g., "Express").
    /// </summary>
    public string? ShortName { get; }

    /// <summary>
    /// The scheduled stop times for this trip, ordered by sequence.
    /// </summary>
    public IReadOnlyList<StopTime> StopTimes => _stopTimes.AsReadOnly();

    /// <summary>
    /// The first stop time of this trip.
    /// </summary>
    public StopTime? FirstStopTime => _stopTimes.Count > 0 ? _stopTimes[0] : null;

    /// <summary>
    /// The last stop time of this trip.
    /// </summary>
    public StopTime? LastStopTime => _stopTimes.Count > 0 ? _stopTimes[^1] : null;

    /// <summary>
    /// The number of stops in this trip.
    /// </summary>
    public int StopCount => _stopTimes.Count;

    /// <summary>
    /// The origin stop (first stop) of this trip.
    /// </summary>
    public Stop? Origin => FirstStopTime?.Stop;

    /// <summary>
    /// The destination stop (last stop) of this trip.
    /// </summary>
    public Stop? Destination => LastStopTime?.Stop;

    /// <summary>
    /// The departure time from the origin.
    /// </summary>
    public TimeOfDay? DepartureTime => FirstStopTime?.Departure;

    /// <summary>
    /// The arrival time at the destination.
    /// </summary>
    public TimeOfDay? ArrivalTime => LastStopTime?.Arrival;

    /// <summary>
    /// The total duration of the trip.
    /// </summary>
    public Duration? TotalDuration
    {
        get
        {
            if (DepartureTime is null || ArrivalTime is null)
                return null;
            return DepartureTime.Value.DurationUntil(ArrivalTime.Value);
        }
    }

    /// <summary>
    /// Creates a new Trip with no stop times.
    /// </summary>
    /// <param name="id">The trip identifier</param>
    /// <param name="headsign">Optional headsign</param>
    /// <param name="shortName">Optional short name</param>
    public Trip(TripId id, string? headsign = null, string? shortName = null)
    {
        Id = id;
        Headsign = headsign;
        ShortName = shortName;
        _stopTimes = new List<StopTime>();
        _usedSequenceNumbers = new HashSet<int>();
    }

    /// <summary>
    /// Creates a new Trip with the specified stop times.
    /// </summary>
    /// <param name="id">The trip identifier</param>
    /// <param name="stopTimes">The stop times to add</param>
    /// <param name="headsign">Optional headsign</param>
    /// <param name="shortName">Optional short name</param>
    /// <exception cref="DuplicateSequenceException">Thrown when duplicate sequence numbers are detected</exception>
    public Trip(TripId id, IEnumerable<StopTime> stopTimes, string? headsign = null, string? shortName = null)
        : this(id, headsign, shortName)
    {
        Guard.Against.Null(stopTimes, nameof(stopTimes));

        foreach (var stopTime in stopTimes)
        {
            AddStopTimeInternal(stopTime);
        }

        SortStopTimes();
    }

    /// <summary>
    /// Adds a stop time to this trip.
    /// </summary>
    /// <param name="stopTime">The stop time to add</param>
    /// <exception cref="ArgumentNullException">Thrown when stopTime is null</exception>
    /// <exception cref="DuplicateSequenceException">Thrown when the sequence number is already used</exception>
    public void AddStopTime(StopTime stopTime)
    {
        AddStopTimeInternal(stopTime);
        SortStopTimes();
    }

    /// <summary>
    /// Adds multiple stop times to this trip.
    /// </summary>
    /// <param name="stopTimes">The stop times to add</param>
    /// <exception cref="ArgumentNullException">Thrown when stopTimes is null</exception>
    /// <exception cref="DuplicateSequenceException">Thrown when duplicate sequence numbers are detected</exception>
    public void AddStopTimes(IEnumerable<StopTime> stopTimes)
    {
        Guard.Against.Null(stopTimes, nameof(stopTimes));

        foreach (var stopTime in stopTimes)
        {
            AddStopTimeInternal(stopTime);
        }

        SortStopTimes();
    }

    private void AddStopTimeInternal(StopTime stopTime)
    {
        Guard.Against.Null(stopTime, nameof(stopTime));

        var sequenceNumber = stopTime.Sequence.Value;
        if (!_usedSequenceNumbers.Add(sequenceNumber))
        {
            throw new DuplicateSequenceException(sequenceNumber);
        }

        _stopTimes.Add(stopTime);
    }

    private void SortStopTimes()
    {
        _stopTimes.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));
    }

    /// <summary>
    /// Gets the stop time at a specific sequence number.
    /// Uses binary search for O(log n) performance since stop times are sorted by sequence.
    /// </summary>
    /// <param name="sequence">The sequence number</param>
    /// <returns>The stop time, or null if not found</returns>
    public StopTime? GetStopTimeAt(StopSequence sequence)
    {
        if (_stopTimes.Count == 0)
            return null;

        // Binary search since list is sorted by sequence
        var left = 0;
        var right = _stopTimes.Count - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var midSequence = _stopTimes[mid].Sequence;

            var comparison = midSequence.CompareTo(sequence);
            if (comparison == 0)
                return _stopTimes[mid];

            if (comparison < 0)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return null;
    }

    /// <summary>
    /// Gets the stop time for a specific stop.
    /// </summary>
    /// <param name="stop">The stop to find</param>
    /// <returns>The stop time, or null if the stop is not part of this trip</returns>
    public StopTime? GetStopTimeFor(Stop stop)
    {
        Guard.Against.Null(stop, nameof(stop));
        return _stopTimes.Find(st => st.Stop.Equals(stop));
    }

    /// <summary>
    /// Checks if this trip serves a specific stop.
    /// </summary>
    /// <param name="stop">The stop to check</param>
    /// <returns>True if the trip stops at this stop</returns>
    public bool ServesStop(Stop stop)
    {
        Guard.Against.Null(stop, nameof(stop));
        return _stopTimes.Any(st => st.Stop.Equals(stop));
    }

    /// <summary>
    /// Gets all stop times between two sequence numbers (inclusive).
    /// Uses binary search to find the starting point for O(log n + k) performance
    /// where k is the number of results.
    /// </summary>
    /// <param name="fromSequence">The starting sequence</param>
    /// <param name="toSequence">The ending sequence</param>
    /// <returns>Stop times in the range</returns>
    public IEnumerable<StopTime> GetStopTimesBetween(StopSequence fromSequence, StopSequence toSequence)
    {
        if (_stopTimes.Count == 0)
            yield break;

        // Binary search to find the first element >= fromSequence
        var startIndex = FindFirstIndexGreaterOrEqual(fromSequence);
        if (startIndex < 0)
            yield break;

        // Iterate from startIndex until we exceed toSequence
        for (var i = startIndex; i < _stopTimes.Count; i++)
        {
            var st = _stopTimes[i];
            if (st.Sequence > toSequence)
                yield break;
            yield return st;
        }
    }

    /// <summary>
    /// Finds the index of the first stop time with sequence >= target using binary search.
    /// </summary>
    private int FindFirstIndexGreaterOrEqual(StopSequence target)
    {
        var left = 0;
        var right = _stopTimes.Count - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var midSequence = _stopTimes[mid].Sequence;

            if (midSequence >= target)
            {
                result = mid;
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a sequence number is already used in this trip.
    /// </summary>
    /// <param name="sequence">The sequence to check</param>
    /// <returns>True if the sequence is already used</returns>
    public bool HasSequence(StopSequence sequence)
    {
        return _usedSequenceNumbers.Contains(sequence.Value);
    }

    public bool Equals(Trip? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Trip other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Trip? left, Trip? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Trip? left, Trip? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        var headsignPart = Headsign is not null ? $" → {Headsign}" : "";
        var stopsPart = StopCount > 0 ? $" ({StopCount} stops)" : "";
        return $"Trip {Id}{headsignPart}{stopsPart}";
    }
}
