namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents a duration of time in minutes.
/// Value object with immutable semantics.
/// </summary>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    /// <summary>
    /// Zero duration.
    /// </summary>
    public static readonly Duration Zero = new(0);

    /// <summary>
    /// One minute duration.
    /// </summary>
    public static readonly Duration OneMinute = new(1);

    /// <summary>
    /// One hour duration.
    /// </summary>
    public static readonly Duration OneHour = new(60);

    /// <summary>
    /// Total minutes in this duration.
    /// </summary>
    public int TotalMinutes { get; }

    /// <summary>
    /// Hours component of the duration.
    /// </summary>
    public int Hours => TotalMinutes / 60;

    /// <summary>
    /// Minutes component (0-59) of the duration.
    /// </summary>
    public int Minutes => TotalMinutes % 60;

    /// <summary>
    /// Total seconds in this duration (for display purposes).
    /// </summary>
    public int TotalSeconds => TotalMinutes * 60;

    /// <summary>
    /// Creates a Duration from total minutes.
    /// </summary>
    /// <param name="totalMinutes">Total minutes (must be non-negative)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minutes is negative</exception>
    private Duration(int totalMinutes)
    {
        if (totalMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(totalMinutes), totalMinutes, "Duration cannot be negative.");

        TotalMinutes = totalMinutes;
    }

    /// <summary>
    /// Creates a Duration from total minutes.
    /// </summary>
    /// <param name="minutes">Total minutes (must be non-negative)</param>
    /// <returns>A new Duration instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minutes is negative</exception>
    public static Duration FromMinutes(int minutes)
    {
        return new Duration(minutes);
    }

    /// <summary>
    /// Creates a Duration from hours and minutes.
    /// </summary>
    /// <param name="hours">Hours (must be non-negative)</param>
    /// <param name="minutes">Minutes (0-59)</param>
    /// <returns>A new Duration instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hours or minutes are invalid</exception>
    public static Duration FromHoursAndMinutes(int hours, int minutes)
    {
        if (hours < 0)
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours cannot be negative.");
        if (minutes < 0 || minutes > 59)
            throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Minutes must be between 0 and 59.");

        return new Duration(hours * 60 + minutes);
    }

    /// <summary>
    /// Creates a Duration from a TimeSpan.
    /// </summary>
    /// <param name="timeSpan">TimeSpan to convert</param>
    /// <returns>A new Duration instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeSpan is negative</exception>
    public static Duration FromTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan, "TimeSpan cannot be negative.");

        return new Duration((int)timeSpan.TotalMinutes);
    }

    /// <summary>
    /// Converts this Duration to a TimeSpan.
    /// </summary>
    /// <returns>Equivalent TimeSpan</returns>
    public TimeSpan ToTimeSpan() => TimeSpan.FromMinutes(TotalMinutes);

    /// <summary>
    /// Adds two durations together.
    /// </summary>
    public Duration Add(Duration other) => new(TotalMinutes + other.TotalMinutes);

    /// <summary>
    /// Subtracts another duration from this one.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if result would be negative</exception>
    public Duration Subtract(Duration other)
    {
        var result = TotalMinutes - other.TotalMinutes;
        if (result < 0)
            throw new InvalidOperationException($"Cannot subtract {other.TotalMinutes} minutes from {TotalMinutes} minutes.");

        return new Duration(result);
    }

    /// <summary>
    /// Multiplies this duration by a factor.
    /// </summary>
    /// <param name="factor">Multiplication factor (must be non-negative)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if factor is negative</exception>
    public Duration Multiply(int factor)
    {
        if (factor < 0)
            throw new ArgumentOutOfRangeException(nameof(factor), factor, "Factor cannot be negative.");

        return new Duration(TotalMinutes * factor);
    }

    public bool Equals(Duration other) => TotalMinutes == other.TotalMinutes;
    public override bool Equals(object? obj) => obj is Duration other && Equals(other);
    public override int GetHashCode() => TotalMinutes;
    public int CompareTo(Duration other) => TotalMinutes.CompareTo(other.TotalMinutes);

    public static bool operator ==(Duration left, Duration right) => left.Equals(right);
    public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
    public static bool operator <(Duration left, Duration right) => left.CompareTo(right) < 0;
    public static bool operator >(Duration left, Duration right) => left.CompareTo(right) > 0;
    public static bool operator <=(Duration left, Duration right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Duration left, Duration right) => left.CompareTo(right) >= 0;

    public static Duration operator +(Duration left, Duration right) => left.Add(right);
    public static Duration operator -(Duration left, Duration right) => left.Subtract(right);
    public static Duration operator *(Duration duration, int factor) => duration.Multiply(factor);
    public static Duration operator *(int factor, Duration duration) => duration.Multiply(factor);

    /// <summary>
    /// Returns string representation in "Xh Ym" format.
    /// </summary>
    public override string ToString()
    {
        if (TotalMinutes == 0)
            return "0m";

        var hours = Hours;
        var minutes = Minutes;

        if (hours == 0)
        {
            // Format: "Xm" (2-3 chars)
            return minutes < 10
                ? string.Create(2, minutes, static (span, m) => { span[0] = (char)('0' + m); span[1] = 'm'; })
                : string.Create(3, minutes, static (span, m) => { span[0] = (char)('0' + m / 10); span[1] = (char)('0' + m % 10); span[2] = 'm'; });
        }

        if (minutes == 0)
        {
            // Format: "Xh" (2-4 chars depending on hours)
            if (hours < 10)
                return string.Create(2, hours, static (span, h) => { span[0] = (char)('0' + h); span[1] = 'h'; });
            if (hours < 100)
                return string.Create(3, hours, static (span, h) => { span[0] = (char)('0' + h / 10); span[1] = (char)('0' + h % 10); span[2] = 'h'; });
            return $"{hours}h";
        }

        // Format: "Xh Ym" - use interpolation for complex cases as it's readable and still efficient
        return $"{hours}h {minutes}m";
    }

    /// <summary>
    /// Returns string representation in "HH:MM" format.
    /// </summary>
    public string ToTimeString()
    {
        return string.Create(5, this, static (span, dur) =>
        {
            int hours = dur.Hours;
            int minutes = dur.Minutes;
            span[0] = (char)('0' + hours / 10);
            span[1] = (char)('0' + hours % 10);
            span[2] = ':';
            span[3] = (char)('0' + minutes / 10);
            span[4] = (char)('0' + minutes % 10);
        });
    }
}
