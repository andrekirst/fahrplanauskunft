using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a stop/station from the GTFS stops.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#stopstxt
/// </summary>
public sealed class GtfsStop
{
    /// <summary>
    /// Required. Identifies a location: stop/platform, station, entrance/exit, generic node, or boarding area.
    /// </summary>
    [Name("stop_id")]
    public string StopId { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Short text or a number that identifies the location for riders.
    /// </summary>
    [Name("stop_code")]
    public string? StopCode { get; set; }

    /// <summary>
    /// Conditionally Required. Name of the location.
    /// Required for locations which are stops (location_type=0), stations (location_type=1), or entrances/exits (location_type=2).
    /// </summary>
    [Name("stop_name")]
    public string? StopName { get; set; }

    /// <summary>
    /// Optional. Description of the location that provides useful, quality information.
    /// </summary>
    [Name("stop_desc")]
    public string? StopDesc { get; set; }

    /// <summary>
    /// Conditionally Required. Latitude of the location.
    /// Required for locations which are stops (location_type=0), stations (location_type=1), or entrances/exits (location_type=2).
    /// </summary>
    [Name("stop_lat")]
    public double? StopLat { get; set; }

    /// <summary>
    /// Conditionally Required. Longitude of the location.
    /// Required for locations which are stops (location_type=0), stations (location_type=1), or entrances/exits (location_type=2).
    /// </summary>
    [Name("stop_lon")]
    public double? StopLon { get; set; }

    /// <summary>
    /// Optional. Identifies the fare zone for a stop.
    /// </summary>
    [Name("zone_id")]
    public string? ZoneId { get; set; }

    /// <summary>
    /// Optional. URL of a web page about the location.
    /// </summary>
    [Name("stop_url")]
    public string? StopUrl { get; set; }

    /// <summary>
    /// Optional. Location type:
    /// 0 (or blank) - Stop/Platform
    /// 1 - Station
    /// 2 - Entrance/Exit
    /// 3 - Generic Node
    /// 4 - Boarding Area
    /// </summary>
    [Name("location_type")]
    public int? LocationType { get; set; }

    /// <summary>
    /// Conditionally Required. Defines hierarchy between locations.
    /// Required for entrances (location_type=2), generic nodes (location_type=3), and boarding areas (location_type=4).
    /// Optional for stops/platforms (location_type=0).
    /// </summary>
    [Name("parent_station")]
    public string? ParentStation { get; set; }

    /// <summary>
    /// Optional. Timezone of the location.
    /// </summary>
    [Name("stop_timezone")]
    public string? StopTimezone { get; set; }

    /// <summary>
    /// Optional. Indicates whether wheelchair boardings are possible:
    /// 0 (or blank) - No accessibility information
    /// 1 - Some vehicles can be boarded by a wheelchair rider
    /// 2 - Wheelchair boarding is not possible
    /// </summary>
    [Name("wheelchair_boarding")]
    public int? WheelchairBoarding { get; set; }

    /// <summary>
    /// Optional. Level of the location (for stations with multiple floors).
    /// References levels.txt level_id.
    /// </summary>
    [Name("level_id")]
    public string? LevelId { get; set; }

    /// <summary>
    /// Optional. Platform identifier for a platform stop (stop belonging to a station).
    /// </summary>
    [Name("platform_code")]
    public string? PlatformCode { get; set; }
}
