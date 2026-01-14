namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for routes in the transit network.
/// Prevents accidental mixing of route IDs with other identifier types.
/// </summary>
public readonly struct RouteId : IEquatable<RouteId>
{
    /// <summary>
    /// The underlying string value of the route ID.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new RouteId.
    /// </summary>
    /// <param name="value">The route ID value (cannot be null or whitespace)</param>
    /// <exception cref="ArgumentException">Thrown when value is null or whitespace</exception>
    public RouteId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Route ID cannot be null or empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a RouteId from a string value.
    /// </summary>
    public static RouteId From(string value) => new(value);

    /// <summary>
    /// Tries to create a RouteId from a string value.
    /// </summary>
    /// <param name="value">The string value</param>
    /// <param name="result">The resulting RouteId if successful</param>
    /// <returns>True if creation succeeded</returns>
    public static bool TryCreate(string? value, out RouteId result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        result = new RouteId(value);
        return true;
    }

    public bool Equals(RouteId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is RouteId other && Equals(other);
    public override int GetHashCode() => Value?.GetHashCode(StringComparison.Ordinal) ?? 0;

    public static bool operator ==(RouteId left, RouteId right) => left.Equals(right);
    public static bool operator !=(RouteId left, RouteId right) => !left.Equals(right);

    /// <summary>
    /// Explicit conversion to string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator string(RouteId id) => id.Value;

    /// <summary>
    /// Explicit conversion from string to prevent accidental implicit conversions.
    /// </summary>
    public static explicit operator RouteId(string value) => new(value);

    public override string ToString() => Value ?? string.Empty;
}
