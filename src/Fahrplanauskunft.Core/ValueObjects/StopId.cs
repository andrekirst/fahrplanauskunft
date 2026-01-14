namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for stops in the transit network.
/// Prevents accidental mixing of stop IDs with other identifier types.
/// </summary>
public readonly struct StopId : IEquatable<StopId>
{
    /// <summary>
    /// The underlying string value of the stop ID.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new StopId.
    /// </summary>
    /// <param name="value">The stop ID value (cannot be null or whitespace)</param>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace</exception>
    public StopId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Stop ID cannot be null or empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a StopId from a string value.
    /// </summary>
    public static StopId From(string value) => new(value);

    /// <summary>
    /// Tries to create a StopId from a string value.
    /// </summary>
    /// <param name="value">The string value</param>
    /// <param name="result">The resulting StopId if successful</param>
    /// <returns>True if creation succeeded</returns>
    public static bool TryCreate(string? value, out StopId result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = new StopId(value);
        return true;
    }

    public bool Equals(StopId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is StopId other && Equals(other);
    public override int GetHashCode() => Value?.GetHashCode(StringComparison.Ordinal) ?? 0;

    public static bool operator ==(StopId left, StopId right) => left.Equals(right);
    public static bool operator !=(StopId left, StopId right) => !left.Equals(right);

    /// <summary>
    /// Explicit conversion to string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator string(StopId id) => id.Value;

    /// <summary>
    /// Explicit conversion from string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator StopId(string value) => new(value);

    public override string ToString() => Value ?? string.Empty;
}
