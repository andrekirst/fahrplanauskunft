using System.Globalization;

namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents geographic coordinates (latitude and longitude).
/// Uses WGS84 coordinate system.
/// </summary>
public readonly struct Coordinates : IEquatable<Coordinates>
{
    /// <summary>
    /// Minimum valid latitude (-90 degrees).
    /// </summary>
    public const double MinLatitude = -90.0;

    /// <summary>
    /// Maximum valid latitude (90 degrees).
    /// </summary>
    public const double MaxLatitude = 90.0;

    /// <summary>
    /// Minimum valid longitude (-180 degrees).
    /// </summary>
    public const double MinLongitude = -180.0;

    /// <summary>
    /// Maximum valid longitude (180 degrees).
    /// </summary>
    public const double MaxLongitude = 180.0;

    /// <summary>
    /// Earth radius in kilometers (mean radius).
    /// </summary>
    public const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Pre-computed constant for degrees to radians conversion.
    /// </summary>
    private const double DegreesToRadiansConstant = Math.PI / 180.0;

    /// <summary>
    /// Latitude in decimal degrees (WGS84).
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Longitude in decimal degrees (WGS84).
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// Creates new Coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees (-90 to 90)</param>
    /// <param name="longitude">Longitude in decimal degrees (-180 to 180)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of valid range</exception>
    public Coordinates(double latitude, double longitude)
    {
        if (latitude < MinLatitude || latitude > MaxLatitude)
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, $"Latitude must be between {MinLatitude} and {MaxLatitude}.");
        if (longitude < MinLongitude || longitude > MaxLongitude)
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, $"Longitude must be between {MinLongitude} and {MaxLongitude}.");
        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be a valid number.");
        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be a valid number.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Creates Coordinates from latitude and longitude values.
    /// </summary>
    public static Coordinates From(double latitude, double longitude) => new(latitude, longitude);

    /// <summary>
    /// Tries to create Coordinates from latitude and longitude values.
    /// </summary>
    /// <param name="latitude">Latitude value</param>
    /// <param name="longitude">Longitude value</param>
    /// <param name="result">The resulting Coordinates if successful</param>
    /// <returns>True if creation succeeded</returns>
    public static bool TryCreate(double latitude, double longitude, out Coordinates result)
    {
        result = default;

        if (latitude < MinLatitude || latitude > MaxLatitude)
            return false;
        if (longitude < MinLongitude || longitude > MaxLongitude)
            return false;
        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
            return false;
        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
            return false;

        result = new Coordinates(latitude, longitude);
        return true;
    }

    /// <summary>
    /// Calculates the distance to another coordinate using the Haversine formula.
    /// </summary>
    /// <param name="other">Target coordinates</param>
    /// <returns>Distance in kilometers</returns>
    public double DistanceToKm(Coordinates other)
    {
        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLatRad = DegreesToRadians(other.Latitude - Latitude);
        var deltaLonRad = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates the distance to another coordinate in meters.
    /// </summary>
    /// <param name="other">Target coordinates</param>
    /// <returns>Distance in meters</returns>
    public double DistanceToMeters(Coordinates other) => DistanceToKm(other) * 1000;

    /// <summary>
    /// Checks if this coordinate is within a given distance of another coordinate.
    /// </summary>
    /// <param name="other">Target coordinates</param>
    /// <param name="distanceKm">Maximum distance in kilometers</param>
    /// <returns>True if within the specified distance</returns>
    public bool IsWithinKm(Coordinates other, double distanceKm) => DistanceToKm(other) <= distanceKm;

    /// <summary>
    /// Checks if this coordinate is within a given distance in meters of another coordinate.
    /// </summary>
    /// <param name="other">Target coordinates</param>
    /// <param name="distanceMeters">Maximum distance in meters</param>
    /// <returns>True if within the specified distance</returns>
    public bool IsWithinMeters(Coordinates other, double distanceMeters) => DistanceToMeters(other) <= distanceMeters;

    private static double DegreesToRadians(double degrees) => degrees * DegreesToRadiansConstant;

    // Use a tolerance for floating-point comparison
    private const double Tolerance = 0.0000001;

    public bool Equals(Coordinates other) =>
        Math.Abs(Latitude - other.Latitude) < Tolerance &&
        Math.Abs(Longitude - other.Longitude) < Tolerance;

    public override bool Equals(object? obj) => obj is Coordinates other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        Math.Round(Latitude, 6),
        Math.Round(Longitude, 6));

    public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);
    public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);

    /// <summary>
    /// Returns coordinates as "lat,lon" string using invariant culture.
    /// </summary>
    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "{0:F6},{1:F6}", Latitude, Longitude);

    /// <summary>
    /// Returns coordinates in ISO 6709 format.
    /// </summary>
    public string ToIso6709String()
    {
        var latDir = Latitude >= 0 ? "N" : "S";
        var lonDir = Longitude >= 0 ? "E" : "W";
        return string.Format(CultureInfo.InvariantCulture, "{0:F6}{1} {2:F6}{3}",
            Math.Abs(Latitude), latDir, Math.Abs(Longitude), lonDir);
    }
}
