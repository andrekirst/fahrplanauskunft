using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a route from the GTFS routes.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#routestxt
/// </summary>
public sealed class GtfsRoute
{
    /// <summary>
    /// Required. Identifies a route.
    /// </summary>
    [Name("route_id")]
    public string RouteId { get; set; } = string.Empty;

    /// <summary>
    /// Conditionally Required. Agency for the specified route.
    /// Required if multiple agencies are defined in agency.txt.
    /// </summary>
    [Name("agency_id")]
    public string? AgencyId { get; set; }

    /// <summary>
    /// Conditionally Required. Short name of a route (e.g., "U1", "32", "S7").
    /// Either route_short_name or route_long_name must be specified, or both.
    /// </summary>
    [Name("route_short_name")]
    public string? RouteShortName { get; set; }

    /// <summary>
    /// Conditionally Required. Full name of a route (e.g., "University Express").
    /// Either route_short_name or route_long_name must be specified, or both.
    /// </summary>
    [Name("route_long_name")]
    public string? RouteLongName { get; set; }

    /// <summary>
    /// Optional. Description of a route that provides useful, quality information.
    /// </summary>
    [Name("route_desc")]
    public string? RouteDesc { get; set; }

    /// <summary>
    /// Required. Indicates the type of transportation used on a route.
    /// Standard values:
    /// 0 - Tram, Streetcar, Light rail
    /// 1 - Subway, Metro
    /// 2 - Rail (long-distance)
    /// 3 - Bus
    /// 4 - Ferry
    /// 5 - Cable tram
    /// 6 - Aerial lift, suspended cable car
    /// 7 - Funicular
    /// 11 - Trolleybus
    /// 12 - Monorail
    /// Extended route types (100-1700) are also supported.
    /// </summary>
    [Name("route_type")]
    public int RouteType { get; set; }

    /// <summary>
    /// Optional. URL of a web page about the particular route.
    /// </summary>
    [Name("route_url")]
    public string? RouteUrl { get; set; }

    /// <summary>
    /// Optional. Route color designation that matches public facing material.
    /// Six-character hexadecimal color (e.g., "00FF00" for green).
    /// Defaults to white (FFFFFF) when omitted or left empty.
    /// </summary>
    [Name("route_color")]
    public string? RouteColor { get; set; }

    /// <summary>
    /// Optional. Legible color to use for text drawn against a background of route_color.
    /// Six-character hexadecimal color (e.g., "000000" for black).
    /// Defaults to black (000000) when omitted or left empty.
    /// </summary>
    [Name("route_text_color")]
    public string? RouteTextColor { get; set; }

    /// <summary>
    /// Optional. Orders the routes in a way which is ideal for presentation to customers.
    /// Routes with smaller route_sort_order values should be displayed first.
    /// </summary>
    [Name("route_sort_order")]
    public int? RouteSortOrder { get; set; }

    /// <summary>
    /// Optional. Indicates that the rider can board the transit vehicle at any point along
    /// the vehicle's travel path. Valid options are:
    /// 0 - Continuous stopping pickup.
    /// 1 (or empty) - No continuous stopping pickup.
    /// 2 - Must phone agency to arrange continuous stopping pickup.
    /// 3 - Must coordinate with driver to arrange continuous stopping pickup.
    /// </summary>
    [Name("continuous_pickup")]
    public int? ContinuousPickup { get; set; }

    /// <summary>
    /// Optional. Indicates that the rider can alight from the transit vehicle at any point along
    /// the vehicle's travel path. Valid options are:
    /// 0 - Continuous stopping drop off.
    /// 1 (or empty) - No continuous stopping drop off.
    /// 2 - Must phone agency to arrange continuous stopping drop off.
    /// 3 - Must coordinate with driver to arrange continuous stopping drop off.
    /// </summary>
    [Name("continuous_drop_off")]
    public int? ContinuousDropOff { get; set; }

    /// <summary>
    /// Optional. Identifies a group of routes. Multiple rows in routes.txt may have the same network_id.
    /// </summary>
    [Name("network_id")]
    public string? NetworkId { get; set; }
}
