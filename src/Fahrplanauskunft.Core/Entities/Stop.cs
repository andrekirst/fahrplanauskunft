using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Core.Entities;

/// <summary>
/// Represents a stop (station/platform) in the transit network.
/// A stop is a location where passengers can board or alight from vehicles.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
    Justification = "Stop is the domain-appropriate name for a transit stop/station per GTFS specification")]
public sealed class Stop : IEquatable<Stop>
{
    /// <summary>
    /// The unique identifier of the stop.
    /// </summary>
    public StopId Id { get; }

    /// <summary>
    /// The name of the stop (e.g., "Central Station", "Market Square").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The geographic coordinates of the stop. Can be null for virtual stops.
    /// </summary>
    public Coordinates? Location { get; }

    /// <summary>
    /// Optional platform or track identifier.
    /// </summary>
    public string? PlatformCode { get; }

    /// <summary>
    /// Indicates whether this stop is wheelchair accessible.
    /// </summary>
    public bool WheelchairAccessible { get; }

    /// <summary>
    /// Creates a new Stop entity.
    /// </summary>
    /// <param name="id">The unique stop identifier</param>
    /// <param name="name">The stop name (cannot be null or whitespace)</param>
    /// <param name="location">The geographic coordinates (optional)</param>
    /// <param name="platformCode">The platform or track code (optional)</param>
    /// <param name="wheelchairAccessible">Wheelchair accessibility status</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null</exception>
    /// <exception cref="ArgumentException">Thrown when name is empty or whitespace</exception>
    public Stop(
        StopId id,
        string name,
        Coordinates? location = null,
        string? platformCode = null,
        bool wheelchairAccessible = false)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name), "Stop name cannot be null or empty.");

        Id = id;
        Name = name;
        Location = location;
        PlatformCode = platformCode;
        WheelchairAccessible = wheelchairAccessible;
    }

    /// <summary>
    /// Creates a simple stop with just an ID and name.
    /// </summary>
    /// <param name="id">The stop ID as a string</param>
    /// <param name="name">The stop name</param>
    /// <returns>A new Stop instance</returns>
    public static Stop Create(string id, string name)
    {
        return new Stop(StopId.From(id), name);
    }

    /// <summary>
    /// Creates a stop with coordinates.
    /// </summary>
    /// <param name="id">The stop ID as a string</param>
    /// <param name="name">The stop name</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <returns>A new Stop instance</returns>
    public static Stop Create(string id, string name, double latitude, double longitude)
    {
        return new Stop(
            StopId.From(id),
            name,
            Coordinates.From(latitude, longitude));
    }

    /// <summary>
    /// Calculates the distance to another stop in kilometers.
    /// </summary>
    /// <param name="other">The target stop</param>
    /// <returns>Distance in kilometers, or null if either stop lacks coordinates</returns>
    public double? DistanceToKm(Stop other)
    {
        if (Location is null || other.Location is null)
            return null;

        return Location.Value.DistanceToKm(other.Location.Value);
    }

    /// <summary>
    /// Checks if this stop is within a given distance of another stop.
    /// </summary>
    /// <param name="other">The target stop</param>
    /// <param name="distanceKm">Maximum distance in kilometers</param>
    /// <returns>True if within distance, false if either stop lacks coordinates or distance exceeds limit</returns>
    public bool IsWithinKm(Stop other, double distanceKm)
    {
        var distance = DistanceToKm(other);
        return distance.HasValue && distance.Value <= distanceKm;
    }

    public bool Equals(Stop? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Stop other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Stop? left, Stop? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Stop? left, Stop? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return PlatformCode is null
            ? $"{Name} ({Id})"
            : $"{Name} Platform {PlatformCode} ({Id})";
    }
}
