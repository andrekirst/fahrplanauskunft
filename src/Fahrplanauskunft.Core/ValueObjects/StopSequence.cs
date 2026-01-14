namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents the sequence number of a stop within a trip.
/// A 1-based index indicating the order of stops along a route.
/// </summary>
public readonly struct StopSequence : IEquatable<StopSequence>, IComparable<StopSequence>
{
    /// <summary>
    /// Minimum valid sequence number (1-based).
    /// </summary>
    public const int MinValue = 1;

    /// <summary>
    /// Maximum valid sequence number.
    /// </summary>
    public const int MaxValue = 10000;

    /// <summary>
    /// The sequence number (1-based).
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Creates a new StopSequence.
    /// </summary>
    /// <param name="value">The sequence number (must be between 1 and MaxValue)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is out of valid range</exception>
    public StopSequence(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Stop sequence must be between {MinValue} and {MaxValue}.");

        Value = value;
    }

    /// <summary>
    /// Creates a StopSequence from an integer value.
    /// </summary>
    public static StopSequence From(int value) => new(value);

    /// <summary>
    /// Tries to create a StopSequence from an integer value.
    /// </summary>
    /// <param name="value">The integer value</param>
    /// <param name="result">The resulting StopSequence if successful</param>
    /// <returns>True if creation succeeded</returns>
    public static bool TryCreate(int value, out StopSequence result)
    {
        result = default;
        if (value < MinValue || value > MaxValue)
            return false;

        result = new StopSequence(value);
        return true;
    }

    /// <summary>
    /// Returns the first stop sequence (1).
    /// </summary>
    public static StopSequence First => new(1);

    /// <summary>
    /// Checks if this is the first stop in the sequence.
    /// </summary>
    public bool IsFirst => Value == MinValue;

    /// <summary>
    /// Returns the next stop sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if already at maximum</exception>
    public StopSequence Next()
    {
        if (Value >= MaxValue)
            throw new InvalidOperationException($"Cannot increment stop sequence beyond {MaxValue}.");

        return new StopSequence(Value + 1);
    }

    /// <summary>
    /// Returns the previous stop sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if already at minimum</exception>
    public StopSequence Previous()
    {
        if (Value <= MinValue)
            throw new InvalidOperationException($"Cannot decrement stop sequence below {MinValue}.");

        return new StopSequence(Value - 1);
    }

    public bool Equals(StopSequence other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is StopSequence other && Equals(other);
    public override int GetHashCode() => Value;
    public int CompareTo(StopSequence other) => Value.CompareTo(other.Value);

    public static bool operator ==(StopSequence left, StopSequence right) => left.Equals(right);
    public static bool operator !=(StopSequence left, StopSequence right) => !left.Equals(right);
    public static bool operator <(StopSequence left, StopSequence right) => left.CompareTo(right) < 0;
    public static bool operator >(StopSequence left, StopSequence right) => left.CompareTo(right) > 0;
    public static bool operator <=(StopSequence left, StopSequence right) => left.CompareTo(right) <= 0;
    public static bool operator >=(StopSequence left, StopSequence right) => left.CompareTo(right) >= 0;

    public static StopSequence operator ++(StopSequence sequence) => sequence.Next();
    public static StopSequence operator --(StopSequence sequence) => sequence.Previous();

    /// <summary>
    /// Explicit conversion to int to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator int(StopSequence sequence) => sequence.Value;

    /// <summary>
    /// Explicit conversion from int to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator StopSequence(int value) => new(value);

    public override string ToString()
    {
        // Optimized for typical sequence values (1-9999)
        // Avoids CultureInfo lookups for simple integer formatting
        var value = Value;
        if (value < 10)
            return string.Create(1, value, static (span, v) => span[0] = (char)('0' + v));
        if (value < 100)
            return string.Create(2, value, static (span, v) => { span[0] = (char)('0' + v / 10); span[1] = (char)('0' + v % 10); });
        if (value < 1000)
            return string.Create(3, value, static (span, v) => { span[0] = (char)('0' + v / 100); span[1] = (char)('0' + (v / 10) % 10); span[2] = (char)('0' + v % 10); });
        if (value < 10000)
            return string.Create(4, value, static (span, v) => { span[0] = (char)('0' + v / 1000); span[1] = (char)('0' + (v / 100) % 10); span[2] = (char)('0' + (v / 10) % 10); span[3] = (char)('0' + v % 10); });
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
