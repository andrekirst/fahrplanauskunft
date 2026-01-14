namespace Fahrplanauskunft.Core.ValueObjects;

/// <summary>
/// Represents the mode of transport for a transit service.
/// Based on GTFS route_type values with additional modes.
/// </summary>
public enum TransportMode
{
    /// <summary>
    /// Tram, streetcar, light rail (GTFS route_type 0).
    /// </summary>
    Tram = 0,

    /// <summary>
    /// Subway, metro (GTFS route_type 1).
    /// </summary>
    Subway = 1,

    /// <summary>
    /// Rail (GTFS route_type 2).
    /// </summary>
    Rail = 2,

    /// <summary>
    /// Bus (GTFS route_type 3).
    /// </summary>
    Bus = 3,

    /// <summary>
    /// Ferry (GTFS route_type 4).
    /// </summary>
    Ferry = 4,

    /// <summary>
    /// Cable tram (GTFS route_type 5).
    /// </summary>
    CableTram = 5,

    /// <summary>
    /// Aerial lift, gondola (GTFS route_type 6).
    /// </summary>
    AerialLift = 6,

    /// <summary>
    /// Funicular (GTFS route_type 7).
    /// </summary>
    Funicular = 7,

    /// <summary>
    /// Trolleybus (GTFS route_type 11).
    /// </summary>
    Trolleybus = 11,

    /// <summary>
    /// Monorail (GTFS route_type 12).
    /// </summary>
    Monorail = 12,

    /// <summary>
    /// Walking/transfer (not a GTFS type - used for connections).
    /// </summary>
    Walk = 100,

    /// <summary>
    /// Unknown or unspecified transport mode.
    /// </summary>
    Unknown = 999
}

/// <summary>
/// Extension methods for TransportMode enum.
/// </summary>
public static class TransportModeExtensions
{
    /// <summary>
    /// Returns a user-friendly display name for the transport mode.
    /// </summary>
    public static string GetDisplayName(this TransportMode mode) => mode switch
    {
        TransportMode.Tram => "Tram",
        TransportMode.Subway => "Subway",
        TransportMode.Rail => "Rail",
        TransportMode.Bus => "Bus",
        TransportMode.Ferry => "Ferry",
        TransportMode.CableTram => "Cable Tram",
        TransportMode.AerialLift => "Aerial Lift",
        TransportMode.Funicular => "Funicular",
        TransportMode.Trolleybus => "Trolleybus",
        TransportMode.Monorail => "Monorail",
        TransportMode.Walk => "Walk",
        TransportMode.Unknown => "Unknown",
        _ => mode.ToString()
    };

    /// <summary>
    /// Returns a German display name for the transport mode.
    /// </summary>
    public static string GetDisplayNameDe(this TransportMode mode) => mode switch
    {
        TransportMode.Tram => "Straßenbahn",
        TransportMode.Subway => "U-Bahn",
        TransportMode.Rail => "Bahn",
        TransportMode.Bus => "Bus",
        TransportMode.Ferry => "Fähre",
        TransportMode.CableTram => "Seilbahn",
        TransportMode.AerialLift => "Luftseilbahn",
        TransportMode.Funicular => "Standseilbahn",
        TransportMode.Trolleybus => "Oberleitungsbus",
        TransportMode.Monorail => "Einschienenbahn",
        TransportMode.Walk => "Fußweg",
        TransportMode.Unknown => "Unbekannt",
        _ => mode.ToString()
    };

    /// <summary>
    /// Returns a single-character abbreviation for the transport mode.
    /// </summary>
    public static char GetAbbreviation(this TransportMode mode) => mode switch
    {
        TransportMode.Tram => 'T',
        TransportMode.Subway => 'U',
        TransportMode.Rail => 'R',
        TransportMode.Bus => 'B',
        TransportMode.Ferry => 'F',
        TransportMode.CableTram => 'C',
        TransportMode.AerialLift => 'A',
        TransportMode.Funicular => 'K', // K for "Kabel"
        TransportMode.Trolleybus => 'O', // O for "Oberleitungsbus"
        TransportMode.Monorail => 'M',
        TransportMode.Walk => 'W',
        TransportMode.Unknown => '?',
        _ => '?'
    };

    /// <summary>
    /// Returns true if the mode represents rail-based transport.
    /// </summary>
    public static bool IsRailBased(this TransportMode mode) => mode switch
    {
        TransportMode.Tram => true,
        TransportMode.Subway => true,
        TransportMode.Rail => true,
        TransportMode.Monorail => true,
        TransportMode.Funicular => true,
        _ => false
    };

    /// <summary>
    /// Returns true if the mode represents road-based transport.
    /// </summary>
    public static bool IsRoadBased(this TransportMode mode) => mode switch
    {
        TransportMode.Bus => true,
        TransportMode.Trolleybus => true,
        _ => false
    };

    /// <summary>
    /// Returns the typical average speed in km/h for the transport mode.
    /// </summary>
    public static int GetTypicalSpeedKmh(this TransportMode mode) => mode switch
    {
        TransportMode.Tram => 20,
        TransportMode.Subway => 35,
        TransportMode.Rail => 60,
        TransportMode.Bus => 25,
        TransportMode.Ferry => 20,
        TransportMode.CableTram => 15,
        TransportMode.AerialLift => 25,
        TransportMode.Funicular => 20,
        TransportMode.Trolleybus => 25,
        TransportMode.Monorail => 40,
        TransportMode.Walk => 5,
        TransportMode.Unknown => 20,
        _ => 20
    };

    /// <summary>
    /// Tries to parse a GTFS route_type integer to a TransportMode.
    /// </summary>
    /// <param name="routeType">GTFS route_type value</param>
    /// <param name="mode">Resulting TransportMode if successful</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool TryFromGtfsRouteType(int routeType, out TransportMode mode)
    {
        if (Enum.IsDefined(typeof(TransportMode), routeType))
        {
            mode = (TransportMode)routeType;
            return true;
        }

        mode = TransportMode.Unknown;
        return false;
    }
}
