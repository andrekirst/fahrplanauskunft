namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents a time of day as minutes since midnight.
/// Value object with immutable semantics.
/// </summary>
public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>
{
    private const int MinutesPerDay = 24 * 60;

    public int MinutesSinceMidnight { get; }

    public int Hours => MinutesSinceMidnight / 60;
    public int Minutes => MinutesSinceMidnight % 60;

    public TimeOfDay(int hours, int minutes)
    {
        if (hours < 0 || hours > 23)
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 0 and 23.");
        if (minutes < 0 || minutes > 59)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be between 0 and 59.");

        MinutesSinceMidnight = hours * 60 + minutes;
    }

    private TimeOfDay(int minutesSinceMidnight)
    {
        MinutesSinceMidnight = minutesSinceMidnight;
    }

    public static TimeOfDay FromMinutesSinceMidnight(int minutes)
    {
        if (minutes < 0 || minutes >= MinutesPerDay)
            throw new ArgumentOutOfRangeException(nameof(minutes), $"Minutes must be between 0 and {MinutesPerDay - 1}.");

        return new TimeOfDay(minutes);
    }

    public bool Equals(TimeOfDay other) => MinutesSinceMidnight == other.MinutesSinceMidnight;
    public override bool Equals(object? obj) => obj is TimeOfDay other && Equals(other);
    public override int GetHashCode() => MinutesSinceMidnight;
    public int CompareTo(TimeOfDay other) => MinutesSinceMidnight.CompareTo(other.MinutesSinceMidnight);

    public static bool operator ==(TimeOfDay left, TimeOfDay right) => left.Equals(right);
    public static bool operator !=(TimeOfDay left, TimeOfDay right) => !left.Equals(right);
    public static bool operator <(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) < 0;
    public static bool operator >(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) > 0;
    public static bool operator <=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TimeOfDay left, TimeOfDay right) => left.CompareTo(right) >= 0;

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
}
