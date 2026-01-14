namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for trips in the transit network.
/// A trip represents a specific journey of a vehicle along a route.
/// Prevents accidental mixing of trip IDs with other identifier types.
/// </summary>
public readonly struct TripId : IEquatable<TripId>
{
    /// <summary>
    /// The underlying string value of the trip ID.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new TripId.
    /// </summary>
    /// <param name="value">The trip ID value (cannot be null or whitespace)</param>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace</exception>
    public TripId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Trip ID cannot be null or empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a TripId from a string value.
    /// </summary>
    public static TripId From(string value) => new(value);

    /// <summary>
    /// Tries to create a TripId from a string value.
    /// </summary>
    /// <param name="value">The string value</param>
    /// <param name="result">The resulting TripId if successful</param>
    /// <returns>True if creation succeeded</returns>
    public static bool TryCreate(string? value, out TripId result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = new TripId(value);
        return true;
    }

    public bool Equals(TripId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is TripId other && Equals(other);
    public override int GetHashCode() => Value?.GetHashCode(StringComparison.Ordinal) ?? 0;

    public static bool operator ==(TripId left, TripId right) => left.Equals(right);
    public static bool operator !=(TripId left, TripId right) => !left.Equals(right);

    /// <summary>
    /// Explicit conversion to string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator string(TripId id) => id.Value;

    /// <summary>
    /// Explicit conversion from string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator TripId(string value) => new(value);

    public override string ToString() => Value ?? string.Empty;
}
