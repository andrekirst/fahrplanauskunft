using CSharpFunctionalExtensions;
using Fahrplanauskunft.Core.Entities;
using Fahrplanauskunft.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Maps GTFS data transfer objects to domain entities with validation.
/// </summary>
/// <remarks>
/// <para>
/// This mapper converts raw GTFS data (parsed from CSV files) into strongly-typed domain entities.
/// Each mapping method performs validation and returns a <see cref="Result{T}"/> to enable
/// graceful error handling during batch processing of thousands of records.
/// </para>
/// <para>
/// The mapper handles GTFS-specific requirements such as:
/// <list type="bullet">
///   <item><description>Extended time formats (e.g., 25:30:00 for trips spanning past midnight)</description></item>
///   <item><description>Route type to TransportMode enum conversion</description></item>
///   <item><description>Coordinate validation for stops</description></item>
///   <item><description>Transfer type interpretation for footpaths</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed partial class GtfsToDomainMapper : IGtfsToDomainMapper
{
    private readonly ILogger<GtfsToDomainMapper> _logger;

    #region Constant Error Messages (avoid repeated allocations)

    private const string ErrorGtfsStopNull = "GtfsStop cannot be null.";
    private const string ErrorGtfsRouteNull = "GtfsRoute cannot be null.";
    private const string ErrorGtfsTripNull = "GtfsTrip cannot be null.";
    private const string ErrorGtfsStopTimeNull = "GtfsStopTime cannot be null.";
    private const string ErrorGtfsTransferNull = "GtfsTransfer cannot be null.";
    private const string ErrorStopEntityNull = "Stop entity cannot be null.";
    private const string ErrorFromStopNull = "From stop cannot be null.";
    private const string ErrorToStopNull = "To stop cannot be null.";
    private const string ErrorStopIdRequired = "stop_id is required and cannot be empty.";
    private const string ErrorRouteIdRequired = "route_id is required and cannot be empty.";
    private const string ErrorTripIdRequired = "trip_id is required and cannot be empty.";
    private const string ErrorTripIdRequiredForStopTime = "trip_id is required for stop time.";
    private const string ErrorFromStopIdRequired = "from_stop_id is required and cannot be empty.";
    private const string ErrorTimeStringEmpty = "Time string cannot be null or empty.";

    #endregion

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping GtfsStop {StopId} to domain entity")]
    private partial void LogMappingStop(string stopId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping GtfsRoute {RouteId} to domain entity")]
    private partial void LogMappingRoute(string routeId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping GtfsTrip {TripId} to domain entity")]
    private partial void LogMappingTrip(string tripId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping GtfsStopTime for trip {TripId}, stop {StopId}, sequence {Sequence}")]
    private partial void LogMappingStopTime(string tripId, string stopId, int sequence);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapping GtfsTransfer from {FromStopId} to {ToStopId}")]
    private partial void LogMappingTransfer(string fromStopId, string toStopId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map GtfsStop {StopId}: {Error}")]
    private partial void LogStopMappingFailed(string stopId, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map GtfsRoute {RouteId}: {Error}")]
    private partial void LogRouteMappingFailed(string routeId, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map GtfsTrip {TripId}: {Error}")]
    private partial void LogTripMappingFailed(string tripId, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map GtfsStopTime for trip {TripId}, sequence {Sequence}: {Error}")]
    private partial void LogStopTimeMappingFailed(string tripId, int sequence, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map GtfsTransfer from {FromStopId} to {ToStopId}: {Error}")]
    private partial void LogTransferMappingFailed(string fromStopId, string toStopId, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to parse GTFS time '{TimeString}': {Error}")]
    private partial void LogTimeParsingFailed(string timeString, string error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown route type {RouteType}, defaulting to Unknown")]
    private partial void LogUnknownRouteType(int routeType);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GtfsToDomainMapper"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging mapping operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public GtfsToDomainMapper(ILogger<GtfsToDomainMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Result<Stop> MapStop(GtfsStop gtfsStop)
    {
        if (gtfsStop is null)
        {
            return Result.Failure<Stop>(ErrorGtfsStopNull);
        }

        // Validate required stop_id first before logging
        if (string.IsNullOrWhiteSpace(gtfsStop.StopId))
        {
            LogStopMappingFailed(gtfsStop.StopId ?? "(null)", ErrorStopIdRequired);
            return Result.Failure<Stop>(ErrorStopIdRequired);
        }

        // Log after stop_id is validated
        LogMappingStop(gtfsStop.StopId);

        // Validate required stop_name (conditionally required for location_type 0, 1, 2)
        // For simplicity, we require it for all stops being mapped to domain entities
        if (string.IsNullOrWhiteSpace(gtfsStop.StopName))
        {
            var error = $"stop_name is required for stop '{gtfsStop.StopId}'.";
            LogStopMappingFailed(gtfsStop.StopId, error);
            return Result.Failure<Stop>(error);
        }

        // Create StopId value object
        if (!StopId.TryCreate(gtfsStop.StopId, out var stopId))
        {
            var error = $"Invalid stop_id '{gtfsStop.StopId}'.";
            LogStopMappingFailed(gtfsStop.StopId, error);
            return Result.Failure<Stop>(error);
        }

        // Create Coordinates if both lat and lon are provided
        Coordinates? location = null;
        if (gtfsStop.StopLat.HasValue && gtfsStop.StopLon.HasValue)
        {
            if (!Coordinates.TryCreate(gtfsStop.StopLat.Value, gtfsStop.StopLon.Value, out var coordinates))
            {
                var error = $"Invalid coordinates (lat: {gtfsStop.StopLat}, lon: {gtfsStop.StopLon}) for stop '{gtfsStop.StopId}'. " +
                           $"Latitude must be between {Coordinates.MinLatitude} and {Coordinates.MaxLatitude}, " +
                           $"longitude must be between {Coordinates.MinLongitude} and {Coordinates.MaxLongitude}.";
                LogStopMappingFailed(gtfsStop.StopId, error);
                return Result.Failure<Stop>(error);
            }
            location = coordinates;
        }

        // Determine wheelchair accessibility
        // GTFS wheelchair_boarding: 0 or null = no info, 1 = accessible, 2 = not accessible
        var wheelchairAccessible = gtfsStop.WheelchairBoarding == 1;

        // Create the Stop entity
        var stop = new Stop(
            stopId,
            gtfsStop.StopName,
            location,
            gtfsStop.PlatformCode,
            wheelchairAccessible);

        return Result.Success(stop);
    }

    /// <inheritdoc />
    public Result<Route> MapRoute(GtfsRoute gtfsRoute)
    {
        if (gtfsRoute is null)
        {
            return Result.Failure<Route>(ErrorGtfsRouteNull);
        }

        // Validate required route_id first before logging
        if (string.IsNullOrWhiteSpace(gtfsRoute.RouteId))
        {
            LogRouteMappingFailed(gtfsRoute.RouteId ?? "(null)", ErrorRouteIdRequired);
            return Result.Failure<Route>(ErrorRouteIdRequired);
        }

        // Log after route_id is validated
        LogMappingRoute(gtfsRoute.RouteId);

        // Validate that at least one of route_short_name or route_long_name is provided (GTFS requirement)
        if (string.IsNullOrWhiteSpace(gtfsRoute.RouteShortName) && string.IsNullOrWhiteSpace(gtfsRoute.RouteLongName))
        {
            var error = $"At least one of route_short_name or route_long_name is required for route '{gtfsRoute.RouteId}'.";
            LogRouteMappingFailed(gtfsRoute.RouteId, error);
            return Result.Failure<Route>(error);
        }

        // Create RouteId value object
        if (!RouteId.TryCreate(gtfsRoute.RouteId, out var routeId))
        {
            var error = $"Invalid route_id '{gtfsRoute.RouteId}'.";
            LogRouteMappingFailed(gtfsRoute.RouteId, error);
            return Result.Failure<Route>(error);
        }

        // Map route_type to TransportMode
        var transportModeResult = MapTransportMode(gtfsRoute.RouteType);
        TransportMode transportMode;
        if (transportModeResult.IsFailure)
        {
            // Log warning but default to Unknown for unknown route types
            LogUnknownRouteType(gtfsRoute.RouteType);
            transportMode = TransportMode.Unknown;
        }
        else
        {
            transportMode = transportModeResult.Value;
        }

        // Determine the short name to use (prefer route_short_name, fall back to route_long_name)
        var shortName = !string.IsNullOrWhiteSpace(gtfsRoute.RouteShortName)
            ? gtfsRoute.RouteShortName
            : gtfsRoute.RouteLongName!; // Safe: we validated at least one is not null

        // Long name is optional (only set if different from short name)
        var longName = !string.IsNullOrWhiteSpace(gtfsRoute.RouteLongName) &&
                       gtfsRoute.RouteLongName != shortName
            ? gtfsRoute.RouteLongName
            : null;

        // Create the Route entity
        var route = new Route(
            routeId,
            shortName,
            transportMode,
            longName,
            gtfsRoute.RouteColor,
            gtfsRoute.RouteTextColor);

        return Result.Success(route);
    }

    /// <inheritdoc />
    public Result<Trip> MapTrip(GtfsTrip gtfsTrip)
    {
        if (gtfsTrip is null)
        {
            return Result.Failure<Trip>(ErrorGtfsTripNull);
        }

        // Validate required trip_id first before logging
        if (string.IsNullOrWhiteSpace(gtfsTrip.TripId))
        {
            LogTripMappingFailed(gtfsTrip.TripId ?? "(null)", ErrorTripIdRequired);
            return Result.Failure<Trip>(ErrorTripIdRequired);
        }

        // Log after trip_id is validated
        LogMappingTrip(gtfsTrip.TripId);

        // Validate required route_id (trips must reference a route)
        if (string.IsNullOrWhiteSpace(gtfsTrip.RouteId))
        {
            var error = $"route_id is required for trip '{gtfsTrip.TripId}'.";
            LogTripMappingFailed(gtfsTrip.TripId, error);
            return Result.Failure<Trip>(error);
        }

        // Validate required service_id (trips must reference a service)
        if (string.IsNullOrWhiteSpace(gtfsTrip.ServiceId))
        {
            var error = $"service_id is required for trip '{gtfsTrip.TripId}'.";
            LogTripMappingFailed(gtfsTrip.TripId, error);
            return Result.Failure<Trip>(error);
        }

        // Create TripId value object
        if (!TripId.TryCreate(gtfsTrip.TripId, out var tripId))
        {
            var error = $"Invalid trip_id '{gtfsTrip.TripId}'.";
            LogTripMappingFailed(gtfsTrip.TripId, error);
            return Result.Failure<Trip>(error);
        }

        // Create the Trip entity
        // Note: StopTimes are added separately after mapping via Trip.AddStopTimes()
        var trip = new Trip(
            tripId,
            gtfsTrip.TripHeadsign,
            gtfsTrip.TripShortName);

        return Result.Success(trip);
    }

    /// <inheritdoc />
    public Result<StopTime> MapStopTime(GtfsStopTime gtfsStopTime, Stop stopEntity)
    {
        if (gtfsStopTime is null)
        {
            return Result.Failure<StopTime>(ErrorGtfsStopTimeNull);
        }

        if (stopEntity is null)
        {
            return Result.Failure<StopTime>(ErrorStopEntityNull);
        }

        // Validate required trip_id first before logging
        if (string.IsNullOrWhiteSpace(gtfsStopTime.TripId))
        {
            LogStopTimeMappingFailed(gtfsStopTime.TripId ?? "(null)", gtfsStopTime.StopSequence, ErrorTripIdRequiredForStopTime);
            return Result.Failure<StopTime>(ErrorTripIdRequiredForStopTime);
        }

        // Validate required stop_id
        if (string.IsNullOrWhiteSpace(gtfsStopTime.StopId))
        {
            var error = $"stop_id is required for stop time in trip '{gtfsStopTime.TripId}'.";
            LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
            return Result.Failure<StopTime>(error);
        }

        // Log after IDs are validated
        LogMappingStopTime(gtfsStopTime.TripId, gtfsStopTime.StopId, gtfsStopTime.StopSequence);

        // Parse arrival time (conditionally required - at least one of arrival/departure must be present)
        TimeOfDay? arrivalTime = null;
        if (!string.IsNullOrWhiteSpace(gtfsStopTime.ArrivalTime))
        {
            var arrivalResult = ParseGtfsTime(gtfsStopTime.ArrivalTime);
            if (arrivalResult.IsFailure)
            {
                var error = $"Invalid arrival_time '{gtfsStopTime.ArrivalTime}' for stop '{gtfsStopTime.StopId}' in trip '{gtfsStopTime.TripId}': {arrivalResult.Error}";
                LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
                return Result.Failure<StopTime>(error);
            }
            arrivalTime = arrivalResult.Value;
        }

        // Parse departure time
        TimeOfDay? departureTime = null;
        if (!string.IsNullOrWhiteSpace(gtfsStopTime.DepartureTime))
        {
            var departureResult = ParseGtfsTime(gtfsStopTime.DepartureTime);
            if (departureResult.IsFailure)
            {
                var error = $"Invalid departure_time '{gtfsStopTime.DepartureTime}' for stop '{gtfsStopTime.StopId}' in trip '{gtfsStopTime.TripId}': {departureResult.Error}";
                LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
                return Result.Failure<StopTime>(error);
            }
            departureTime = departureResult.Value;
        }

        // At least one of arrival_time or departure_time must be provided
        if (!arrivalTime.HasValue && !departureTime.HasValue)
        {
            var error = $"At least one of arrival_time or departure_time is required for stop '{gtfsStopTime.StopId}' in trip '{gtfsStopTime.TripId}'.";
            LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
            return Result.Failure<StopTime>(error);
        }

        // If only one time is provided, use it for both arrival and departure
        var arrival = arrivalTime ?? departureTime!.Value;
        var departure = departureTime ?? arrivalTime!.Value;

        // Create StopSequence value object
        // GTFS stop_sequence can be 0-based or 1-based depending on the feed.
        // StopSequence in our domain is 1-based, so we add 1 if the value is 0
        var sequenceValue = gtfsStopTime.StopSequence;
        if (sequenceValue < StopSequence.MinValue)
        {
            // Convert 0-based to 1-based
            sequenceValue = sequenceValue + 1;
        }

        if (!StopSequence.TryCreate(sequenceValue, out var stopSequence))
        {
            var error = $"Invalid stop_sequence '{gtfsStopTime.StopSequence}' for stop '{gtfsStopTime.StopId}' in trip '{gtfsStopTime.TripId}'. Must be between {StopSequence.MinValue} and {StopSequence.MaxValue}.";
            LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
            return Result.Failure<StopTime>(error);
        }

        // Determine pickup and drop-off availability
        // GTFS pickup_type: 0 or null = regularly scheduled, 1 = none, 2 = phone agency, 3 = coordinate with driver
        // We treat 0 and null as allowed, anything else as not allowed for standard routing
        var pickupAllowed = gtfsStopTime.PickupType is null or 0;

        // GTFS drop_off_type: same semantics as pickup_type
        var dropOffAllowed = gtfsStopTime.DropOffType is null or 0;

        try
        {
            var stopTime = new StopTime(
                stopEntity,
                stopSequence,
                arrival,
                departure,
                pickupAllowed,
                dropOffAllowed);

            return Result.Success(stopTime);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
        {
            var error = $"Failed to create StopTime for stop '{gtfsStopTime.StopId}' in trip '{gtfsStopTime.TripId}': {ex.Message}";
            LogStopTimeMappingFailed(gtfsStopTime.TripId, gtfsStopTime.StopSequence, error);
            return Result.Failure<StopTime>(error);
        }
    }

    /// <inheritdoc />
    public Result<Footpath> MapTransfer(GtfsTransfer gtfsTransfer, Stop fromStop, Stop toStop)
    {
        if (gtfsTransfer is null)
        {
            return Result.Failure<Footpath>(ErrorGtfsTransferNull);
        }

        if (fromStop is null)
        {
            return Result.Failure<Footpath>(ErrorFromStopNull);
        }

        if (toStop is null)
        {
            return Result.Failure<Footpath>(ErrorToStopNull);
        }

        // Validate required from_stop_id first before logging
        if (string.IsNullOrWhiteSpace(gtfsTransfer.FromStopId))
        {
            LogTransferMappingFailed(gtfsTransfer.FromStopId ?? "(null)", gtfsTransfer.ToStopId ?? "(null)", ErrorFromStopIdRequired);
            return Result.Failure<Footpath>(ErrorFromStopIdRequired);
        }

        // Validate required to_stop_id
        if (string.IsNullOrWhiteSpace(gtfsTransfer.ToStopId))
        {
            var error = $"to_stop_id is required for transfer from '{gtfsTransfer.FromStopId}'.";
            LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId ?? "(null)", error);
            return Result.Failure<Footpath>(error);
        }

        // Log after IDs are validated
        LogMappingTransfer(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId);

        // Validate no self-loop: origin and destination must be different
        if (gtfsTransfer.FromStopId == gtfsTransfer.ToStopId)
        {
            var error = $"Transfer cannot have the same from_stop_id and to_stop_id: '{gtfsTransfer.FromStopId}'.";
            LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, error);
            return Result.Failure<Footpath>(error);
        }

        // Handle transfer_type
        // 0 - Recommended transfer point between routes
        // 1 - Timed transfer point
        // 2 - Transfer requires a minimum amount of time
        // 3 - Transfers are not possible at this location
        // 4 - Passengers can stay onboard (in-seat transfer)
        // 5 - In-seat transfers are not allowed
        switch (gtfsTransfer.TransferType)
        {
            case 3:
                // Transfers not possible - return failure
                var error = $"Transfers are not possible between '{gtfsTransfer.FromStopId}' and '{gtfsTransfer.ToStopId}' (transfer_type = 3).";
                LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, error);
                return Result.Failure<Footpath>(error);

            case 4:
            case 5:
                // In-seat transfers don't represent walking transfers
                // Return failure as these don't map to Footpath entities
                var inSeatError = $"In-seat transfer types (4, 5) do not map to Footpath entities. Transfer from '{gtfsTransfer.FromStopId}' to '{gtfsTransfer.ToStopId}' has transfer_type = {gtfsTransfer.TransferType}.";
                LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, inSeatError);
                return Result.Failure<Footpath>(inSeatError);
        }

        // Determine duration based on transfer type and min_transfer_time
        Duration duration;
        if (gtfsTransfer.TransferType == 2)
        {
            // Type 2: Transfer requires minimum time - min_transfer_time should be specified
            if (!gtfsTransfer.MinTransferTime.HasValue || gtfsTransfer.MinTransferTime.Value <= 0)
            {
                var minTimeError = $"min_transfer_time is required for transfer_type 2 between '{gtfsTransfer.FromStopId}' and '{gtfsTransfer.ToStopId}'.";
                LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, minTimeError);
                return Result.Failure<Footpath>(minTimeError);
            }

            // Convert seconds to minutes (round up to ensure minimum time is met)
            var minutes = (int)Math.Ceiling(gtfsTransfer.MinTransferTime.Value / 60.0);
            duration = Duration.FromMinutes(minutes);
        }
        else
        {
            // Type 0 or 1: Recommended or timed transfer
            // Use min_transfer_time if provided, otherwise default to a reasonable walking time
            if (gtfsTransfer.MinTransferTime.HasValue && gtfsTransfer.MinTransferTime.Value > 0)
            {
                var minutes = (int)Math.Ceiling(gtfsTransfer.MinTransferTime.Value / 60.0);
                duration = Duration.FromMinutes(minutes);
            }
            else
            {
                // Default to 2 minutes for recommended/timed transfers without specified time
                duration = Duration.FromMinutes(2);
            }
        }

        // Determine wheelchair accessibility based on both stops
        // A footpath is only wheelchair accessible if both the origin and destination stops are accessible
        var wheelchairAccessible = fromStop.WheelchairAccessible && toStop.WheelchairAccessible;

        try
        {
            var footpath = new Footpath(
                fromStop,
                toStop,
                duration,
                wheelchairAccessible);

            return Result.Success(footpath);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
        {
            var createError = $"Failed to create Footpath from '{gtfsTransfer.FromStopId}' to '{gtfsTransfer.ToStopId}': {ex.Message}";
            LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, createError);
            return Result.Failure<Footpath>(createError);
        }
        catch (Core.Exceptions.SelfLoopException)
        {
            var selfLoopError = $"Cannot create footpath: from stop and to stop are the same entity.";
            LogTransferMappingFailed(gtfsTransfer.FromStopId, gtfsTransfer.ToStopId, selfLoopError);
            return Result.Failure<Footpath>(selfLoopError);
        }
    }

    /// <inheritdoc />
    public Result<TransportMode> MapTransportMode(int routeType)
    {
        // Standard GTFS route types (0-12)
        // See: https://gtfs.org/schedule/reference/#routestxt
        if (TransportModeExtensions.TryFromGtfsRouteType(routeType, out var mode))
        {
            return Result.Success(mode);
        }

        // Handle extended route types (100-1700)
        // Map them to the closest standard type
        var extendedMode = routeType switch
        {
            // Railway Service (100-199) -> Rail
            >= 100 and < 200 => TransportMode.Rail,
            // Coach Service (200-299) -> Bus
            >= 200 and < 300 => TransportMode.Bus,
            // Suburban Railway Service (300-399) -> Rail
            >= 300 and < 400 => TransportMode.Rail,
            // Urban Railway Service (400-499) -> Subway
            >= 400 and < 500 => TransportMode.Subway,
            // Metro Service (500-599) -> Subway
            >= 500 and < 600 => TransportMode.Subway,
            // Underground Service (600-699) -> Subway
            >= 600 and < 700 => TransportMode.Subway,
            // Tram Service (700-799) -> Tram
            >= 700 and < 800 => TransportMode.Tram,
            // Bus Service (800-899) -> Bus
            >= 800 and < 900 => TransportMode.Bus,
            // Trolleybus Service (900-999) -> Trolleybus
            >= 900 and < 1000 => TransportMode.Trolleybus,
            // Water Transport Service (1000-1099) -> Ferry
            >= 1000 and < 1100 => TransportMode.Ferry,
            // Air Service (1100-1199) -> Unknown (not supported)
            >= 1100 and < 1200 => TransportMode.Unknown,
            // Ferry Service (1200-1299) -> Ferry
            >= 1200 and < 1300 => TransportMode.Ferry,
            // Aerial Lift Service (1300-1399) -> AerialLift
            >= 1300 and < 1400 => TransportMode.AerialLift,
            // Funicular Service (1400-1499) -> Funicular
            >= 1400 and < 1500 => TransportMode.Funicular,
            // Taxi Service (1500-1599) -> Unknown (not supported)
            >= 1500 and < 1600 => TransportMode.Unknown,
            // Miscellaneous Service (1600-1699) -> Unknown
            >= 1600 and < 1700 => TransportMode.Unknown,
            // Cable Car (1700-1799) -> CableTram
            >= 1700 and < 1800 => TransportMode.CableTram,
            // Unknown route type
            _ => (TransportMode?)null
        };

        if (extendedMode.HasValue)
        {
            return Result.Success(extendedMode.Value);
        }

        // Unknown route type - return failure to allow caller to decide how to handle
        return Result.Failure<TransportMode>($"Unknown GTFS route_type: {routeType}.");
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method is optimized for high throughput using span-based parsing
    /// to avoid heap allocations when processing large GTFS feeds (100K+ stop times).
    /// </remarks>
    public Result<TimeOfDay> ParseGtfsTime(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
        {
            return Result.Failure<TimeOfDay>(ErrorTimeStringEmpty);
        }

        // Use span-based parsing to avoid array allocation from Split()
        var span = timeString.AsSpan();

        // Find first colon (separates hours from minutes)
        var firstColonIndex = span.IndexOf(':');
        if (firstColonIndex < 1)
        {
            var error = $"Invalid time format '{timeString}'. Expected 'HH:MM:SS', 'H:MM:SS', 'HH:MM', or 'H:MM'.";
            LogTimeParsingFailed(timeString, error);
            return Result.Failure<TimeOfDay>(error);
        }

        // Parse hours
        var hoursSpan = span[..firstColonIndex];
        if (!int.TryParse(hoursSpan, out var hours))
        {
            var error = $"Invalid hours component in time '{timeString}'.";
            LogTimeParsingFailed(timeString, error);
            return Result.Failure<TimeOfDay>(error);
        }

        // Validate hours range (0-47 for extended transit times)
        if (hours < 0 || hours > 47)
        {
            var error = $"Hours component in time '{timeString}' must be between 0 and 47.";
            LogTimeParsingFailed(timeString, error);
            return Result.Failure<TimeOfDay>(error);
        }

        // Get remaining part after hours
        var afterHours = span[(firstColonIndex + 1)..];

        // Find second colon (separates minutes from seconds, if present)
        var secondColonIndex = afterHours.IndexOf(':');

        ReadOnlySpan<char> minutesSpan;
        ReadOnlySpan<char> secondsSpan = default;

        if (secondColonIndex >= 0)
        {
            // Format: HH:MM:SS
            minutesSpan = afterHours[..secondColonIndex];
            secondsSpan = afterHours[(secondColonIndex + 1)..];
        }
        else
        {
            // Format: HH:MM
            minutesSpan = afterHours;
        }

        // Parse minutes
        if (minutesSpan.Length == 0 || !int.TryParse(minutesSpan, out var minutes))
        {
            var error = $"Invalid minutes component in time '{timeString}'.";
            LogTimeParsingFailed(timeString, error);
            return Result.Failure<TimeOfDay>(error);
        }

        // Validate minutes range
        if (minutes < 0 || minutes > 59)
        {
            var error = $"Minutes component in time '{timeString}' must be between 0 and 59.";
            LogTimeParsingFailed(timeString, error);
            return Result.Failure<TimeOfDay>(error);
        }

        // Parse and validate seconds (if present)
        if (secondsSpan.Length > 0)
        {
            if (!int.TryParse(secondsSpan, out var seconds))
            {
                var error = $"Invalid seconds component in time '{timeString}'.";
                LogTimeParsingFailed(timeString, error);
                return Result.Failure<TimeOfDay>(error);
            }

            // Validate seconds range
            if (seconds < 0 || seconds > 59)
            {
                var error = $"Seconds component in time '{timeString}' must be between 0 and 59.";
                LogTimeParsingFailed(timeString, error);
                return Result.Failure<TimeOfDay>(error);
            }
            // Note: Seconds are validated but ignored (TimeOfDay has minute-level granularity)
        }

        // Create TimeOfDay directly without try-catch since we've already validated ranges
        var timeOfDay = new TimeOfDay(hours, minutes);
        return Result.Success(timeOfDay);
    }
}
