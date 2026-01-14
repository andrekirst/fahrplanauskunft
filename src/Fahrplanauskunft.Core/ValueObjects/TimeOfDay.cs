namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents a time of day as total minutes, supporting extended hours beyond 24:00.
/// In transit scheduling, times like "25:30" represent 1:30 AM the next day.
/// Value object with immutable semantics.
/// </summary>
public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>
{
    /// <summary>
    /// Minutes in a standard 24-hour day.
    /// </summary>
    public const int MinutesPerDay = 24 * 60;

    /// <summary>
    /// Maximum allowed minutes (48 hours - allows for next-day extended times).
    /// </summary>
    public const int MaxMinutes = 48 * 60;

    /// <summary>
    /// Total minutes since midnight (can exceed 24 hours for extended transit times).
    /// </summary>
    public int TotalMinutes { get; }

    /// <summary>
    /// Hours component (can exceed 23 for extended times like 25:30).
    /// </summary>
    public int Hours => TotalMinutes / 60;

    /// <summary>
    /// Minutes component (0-59).
    /// </summary>
    public int Minutes => TotalMinutes % 60;

    /// <summary>
    /// Returns true if this time extends beyond midnight (24:00 or later).
    /// </summary>
    public bool IsExtended => TotalMinutes >= MinutesPerDay;

    /// <summary>
    /// Returns the normalized time (modulo 24 hours) for display purposes.
    /// </summary>
    public TimeOfDay Normalized => IsExtended
        ? FromTotalMinutes(TotalMinutes % MinutesPerDay)
        : this;

    /// <summary>
    /// Creates a TimeOfDay from hours and minutes.
    /// Supports extended hours (0-47) for transit scheduling.
    /// </summary>
    /// <param name="hours">Hours (0-47 for extended transit times)</param>
    /// <param name="minutes">Minutes (0-59)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hours or minutes are out of valid range</exception>
    public TimeOfDay(int hours, int minutes)
    {
        if (hours < 0 || hours > 47)
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours must be between 0 and 47.");
        if (minutes < 0 || minutes > 59)
            throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Minutes must be between 0 and 59.");

        TotalMinutes = hours * 60 + minutes;
    }

    private TimeOfDay(int totalMinutes)
    {
        TotalMinutes = totalMinutes;
    }

    /// <summary>
    /// Creates a TimeOfDay from total minutes since midnight.
    /// </summary>
    /// <param name="minutes">Total minutes (0 to MaxMinutes-1)</param>
    /// <returns>A new TimeOfDay instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minutes are out of valid range</exception>
    public static TimeOfDay FromTotalMinutes(int minutes)
    {
        if (minutes < 0 || minutes >= MaxMinutes)
            throw new ArgumentOutOfRangeException(nameof(minutes), minutes, $"Minutes must be between 0 and {MaxMinutes - 1}.");

        return new TimeOfDay(minutes);
    }

    /// <summary>
    /// Parses a time string in format "HH:MM" or "H:MM".
    /// Supports extended hours like "25:30".
    /// </summary>
    /// <param name="timeString">Time string to parse</param>
    /// <returns>Parsed TimeOfDay</returns>
    /// <exception cref="FormatException">Thrown when the string format is invalid</exception>
    public static TimeOfDay Parse(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            throw new FormatException("Time string cannot be null or empty.");

        if (!TryParseCore(timeString.AsSpan(), out var hours, out var minutes))
            throw new FormatException($"Invalid time format: '{timeString}'. Expected 'HH:MM' or 'H:MM'.");

        return new TimeOfDay(hours, minutes);
    }

    /// <summary>
    /// Tries to parse a time string, returning a boolean indicating success.
    /// </summary>
    /// <param name="timeString">Time string to parse</param>
    /// <param name="result">Parsed result if successful</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryParse(string? timeString, out TimeOfDay result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(timeString))
            return false;

        if (!TryParseCore(timeString.AsSpan(), out var hours, out var minutes))
            return false;

        if (hours < 0 || hours > 47 || minutes < 0 || minutes > 59)
            return false;

        result = new TimeOfDay(hours, minutes);
        return true;
    }

    /// <summary>
    /// Core parsing logic using span to avoid allocations.
    /// </summary>
    private static bool TryParseCore(ReadOnlySpan<char> span, out int hours, out int minutes)
    {
        hours = 0;
        minutes = 0;

        // Find the colon separator
        var colonIndex = span.IndexOf(':');
        if (colonIndex < 1 || colonIndex >= span.Length - 1)
            return false;

        // Parse hours (before colon)
        var hoursPart = span[..colonIndex];
        if (!int.TryParse(hoursPart, out hours))
            return false;

        // Parse minutes (after colon)
        var minutesPart = span[(colonIndex + 1)..];
        if (!int.TryParse(minutesPart, out minutes))
            return false;

        return true;
    }

    /// <summary>
    /// Adds a duration to this time.
    /// </summary>
    /// <param name="duration">Duration to add</param>
    /// <returns>New TimeOfDay with duration added</returns>
    /// <exception cref="InvalidOperationException">Thrown if result would exceed maximum allowed time</exception>
    public TimeOfDay Add(Duration duration)
    {
        var newMinutes = TotalMinutes + duration.TotalMinutes;
        if (newMinutes >= MaxMinutes)
            throw new InvalidOperationException($"Resulting time {newMinutes} minutes exceeds maximum allowed {MaxMinutes - 1} minutes.");

        return FromTotalMinutes(newMinutes);
    }

    /// <summary>
    /// Subtracts a duration from this time.
    /// </summary>
    /// <param name="duration">Duration to subtract</param>
    /// <returns>New TimeOfDay with duration subtracted</returns>
    /// <exception cref="InvalidOperationException">Thrown if result would be negative</exception>
    public TimeOfDay Subtract(Duration duration)
    {
        var newMinutes = TotalMinutes - duration.TotalMinutes;
        if (newMinutes < 0)
            throw new InvalidOperationException($"Resulting time would be negative ({newMinutes} minutes).");

        return FromTotalMinutes(newMinutes);
    }

    /// <summary>
    /// Calculates the duration between this time and another time.
    /// Returns a positive duration representing the absolute difference.
    /// </summary>
    /// <param name="other">The other time</param>
    /// <returns>Duration between the two times</returns>
    public Duration DurationUntil(TimeOfDay other)
    {
        var diff = other.TotalMinutes - TotalMinutes;
        return Duration.FromMinutes(Math.Abs(diff));
    }

    /// <summary>
    /// Calculates the forward duration from this time to another time,
    /// handling midnight crossover. If other is earlier, assumes next day.
    /// </summary>
    /// <param name="other">Target time</param>
    /// <returns>Forward duration to reach target time</returns>
    public Duration ForwardDurationTo(TimeOfDay other)
    {
        var diff = other.TotalMinutes - TotalMinutes;
        if (diff < 0)
        {
            // Handle midnight crossover - add 24 hours
            diff += MinutesPerDay;
        }
        return Duration.FromMinutes(diff);
    }

    public bool Equals(TimeOfDay other) => TotalMinutes == other.TotalMinutes;
    public override bool Equals(object? obj) => obj is TimeOfDay other && Equals(other);
    public override int GetHashCode() => TotalMinutes;
    public int CompareTo(TimeOfDay other) => TotalMinutes.CompareTo(other.TotalMinutes);

    public static bool operator ==(TimeOfDay left, TimeOfDay right) => left.Equals(right);
    public static bool operator !=(TimeOfDay left, TimeOfDay right) => !left.Equals(right);
    public static bool operator <(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) < 0;
    public static bool operator >(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) > 0;
    public static bool operator <=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) >= 0;

    public static TimeOfDay operator +(TimeOfDay time, Duration duration) => time.Add(duration);
    public static TimeOfDay operator -(TimeOfDay time, Duration duration) => time.Subtract(duration);

    /// <summary>
    /// Returns string in "HH:MM" format. Extended hours are shown as-is (e.g., "25:30").
    /// </summary>
    public override string ToString()
    {
        return string.Create(5, this, static (span, time) =>
        {
            int hours = time.Hours;
            int minutes = time.Minutes;
            span[0] = (char)('0' + hours / 10);
            span[1] = (char)('0' + hours % 10);
            span[2] = ':';
            span[3] = (char)('0' + minutes / 10);
            span[4] = (char)('0' + minutes % 10);
        });
    }

    /// <summary>
    /// Returns the time in standard 24-hour format, normalizing extended times.
    /// E.g., "25:30" becomes "01:30".
    /// </summary>
    public string ToNormalizedString() => Normalized.ToString();
}
