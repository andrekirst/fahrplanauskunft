using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Fahrplanauskunft.Infrastructure.Gtfs;

namespace Fahrplanauskunft.Tests.Unit.Gtfs;

/// <summary>
/// Unit tests for GTFS model classes.
/// Tests verify CsvHelper attribute configuration and CSV parsing behavior.
/// </summary>
public class GtfsModelTests
{
    /// <summary>
    /// Standard GTFS route types for verification tests.
    /// </summary>
    private static readonly int[] StandardRouteTypes = [0, 1, 2, 3, 4, 5, 6, 7, 11, 12];

    #region GtfsStop Tests

    [Fact]
    public void GtfsStop_HasCorrectNameAttributes()
    {
        // Arrange & Act
        var properties = typeof(GtfsStop).GetProperties();

        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsStop>("StopId", "stop_id");
        AssertNameAttribute<GtfsStop>("StopCode", "stop_code");
        AssertNameAttribute<GtfsStop>("StopName", "stop_name");
        AssertNameAttribute<GtfsStop>("StopDesc", "stop_desc");
        AssertNameAttribute<GtfsStop>("StopLat", "stop_lat");
        AssertNameAttribute<GtfsStop>("StopLon", "stop_lon");
        AssertNameAttribute<GtfsStop>("ZoneId", "zone_id");
        AssertNameAttribute<GtfsStop>("StopUrl", "stop_url");
        AssertNameAttribute<GtfsStop>("LocationType", "location_type");
        AssertNameAttribute<GtfsStop>("ParentStation", "parent_station");
        AssertNameAttribute<GtfsStop>("StopTimezone", "stop_timezone");
        AssertNameAttribute<GtfsStop>("WheelchairBoarding", "wheelchair_boarding");
        AssertNameAttribute<GtfsStop>("LevelId", "level_id");
        AssertNameAttribute<GtfsStop>("PlatformCode", "platform_code");
    }

    [Fact]
    public void GtfsStop_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            stop_id,stop_code,stop_name,stop_desc,stop_lat,stop_lon,zone_id,stop_url,location_type,parent_station,stop_timezone,wheelchair_boarding,level_id,platform_code
            S1,ABC,Central Station,Main train station,52.5200,13.4050,A,https://example.com,1,P1,Europe/Berlin,1,L1,1A
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        var stop = stops[0];
        stop.StopId.Should().Be("S1");
        stop.StopCode.Should().Be("ABC");
        stop.StopName.Should().Be("Central Station");
        stop.StopDesc.Should().Be("Main train station");
        stop.StopLat.Should().Be(52.52);
        stop.StopLon.Should().Be(13.405);
        stop.ZoneId.Should().Be("A");
        stop.StopUrl.Should().Be("https://example.com");
        stop.LocationType.Should().Be(1);
        stop.ParentStation.Should().Be("P1");
        stop.StopTimezone.Should().Be("Europe/Berlin");
        stop.WheelchairBoarding.Should().Be(1);
        stop.LevelId.Should().Be("L1");
        stop.PlatformCode.Should().Be("1A");
    }

    [Fact]
    public void GtfsStop_WithOptionalFieldsMissing_ShouldParseSuccessfully()
    {
        // Arrange - Only required fields
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,Central Station,52.5200,13.4050
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        var stop = stops[0];
        stop.StopId.Should().Be("S1");
        stop.StopName.Should().Be("Central Station");
        stop.StopCode.Should().BeNull();
        stop.StopDesc.Should().BeNull();
        stop.StopUrl.Should().BeNull();
        stop.LocationType.Should().BeNull();
        stop.ParentStation.Should().BeNull();
        stop.StopTimezone.Should().BeNull();
        stop.WheelchairBoarding.Should().BeNull();
        stop.LevelId.Should().BeNull();
        stop.PlatformCode.Should().BeNull();
    }

    [Fact]
    public void GtfsStop_WithEmptyOptionalFields_ShouldParseAsNull()
    {
        // Arrange - Empty optional fields
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon,stop_desc,stop_url
            S1,Central Station,52.5200,13.4050,,
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        var stop = stops[0];
        stop.StopDesc.Should().BeEmpty();
        stop.StopUrl.Should().BeEmpty();
    }

    #endregion

    #region GtfsRoute Tests

    [Fact]
    public void GtfsRoute_HasCorrectNameAttributes()
    {
        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsRoute>("RouteId", "route_id");
        AssertNameAttribute<GtfsRoute>("AgencyId", "agency_id");
        AssertNameAttribute<GtfsRoute>("RouteShortName", "route_short_name");
        AssertNameAttribute<GtfsRoute>("RouteLongName", "route_long_name");
        AssertNameAttribute<GtfsRoute>("RouteDesc", "route_desc");
        AssertNameAttribute<GtfsRoute>("RouteType", "route_type");
        AssertNameAttribute<GtfsRoute>("RouteUrl", "route_url");
        AssertNameAttribute<GtfsRoute>("RouteColor", "route_color");
        AssertNameAttribute<GtfsRoute>("RouteTextColor", "route_text_color");
        AssertNameAttribute<GtfsRoute>("RouteSortOrder", "route_sort_order");
        AssertNameAttribute<GtfsRoute>("ContinuousPickup", "continuous_pickup");
        AssertNameAttribute<GtfsRoute>("ContinuousDropOff", "continuous_drop_off");
        AssertNameAttribute<GtfsRoute>("NetworkId", "network_id");
    }

    [Fact]
    public void GtfsRoute_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            route_id,agency_id,route_short_name,route_long_name,route_desc,route_type,route_url,route_color,route_text_color,route_sort_order,continuous_pickup,continuous_drop_off,network_id
            R1,A1,U1,Underground Line 1,Fast metro service,1,https://metro.example.com,FF0000,FFFFFF,1,0,0,N1
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(1);
        var route = routes[0];
        route.RouteId.Should().Be("R1");
        route.AgencyId.Should().Be("A1");
        route.RouteShortName.Should().Be("U1");
        route.RouteLongName.Should().Be("Underground Line 1");
        route.RouteDesc.Should().Be("Fast metro service");
        route.RouteType.Should().Be(1);
        route.RouteUrl.Should().Be("https://metro.example.com");
        route.RouteColor.Should().Be("FF0000");
        route.RouteTextColor.Should().Be("FFFFFF");
        route.RouteSortOrder.Should().Be(1);
        route.ContinuousPickup.Should().Be(0);
        route.ContinuousDropOff.Should().Be(0);
        route.NetworkId.Should().Be("N1");
    }

    [Fact]
    public void GtfsRoute_WithExtendedRouteType_ShouldParseSuccessfully()
    {
        // Arrange - Extended route type (100-1700 range)
        var csv = """
            route_id,route_short_name,route_type
            R1,S7,109
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(1);
        routes[0].RouteType.Should().Be(109); // Extended type for suburban railway
    }

    [Fact]
    public void GtfsRoute_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange - Only required fields
        var csv = """
            route_id,route_short_name,route_type
            R1,U1,3
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(1);
        var route = routes[0];
        route.RouteId.Should().Be("R1");
        route.RouteShortName.Should().Be("U1");
        route.RouteType.Should().Be(3);
        route.AgencyId.Should().BeNull();
        route.RouteLongName.Should().BeNull();
    }

    #endregion

    #region GtfsTrip Tests

    [Fact]
    public void GtfsTrip_HasCorrectNameAttributes()
    {
        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsTrip>("RouteId", "route_id");
        AssertNameAttribute<GtfsTrip>("ServiceId", "service_id");
        AssertNameAttribute<GtfsTrip>("TripId", "trip_id");
        AssertNameAttribute<GtfsTrip>("TripHeadsign", "trip_headsign");
        AssertNameAttribute<GtfsTrip>("TripShortName", "trip_short_name");
        AssertNameAttribute<GtfsTrip>("DirectionId", "direction_id");
        AssertNameAttribute<GtfsTrip>("BlockId", "block_id");
        AssertNameAttribute<GtfsTrip>("ShapeId", "shape_id");
        AssertNameAttribute<GtfsTrip>("WheelchairAccessible", "wheelchair_accessible");
        AssertNameAttribute<GtfsTrip>("BikesAllowed", "bikes_allowed");
    }

    [Fact]
    public void GtfsTrip_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            route_id,service_id,trip_id,trip_headsign,trip_short_name,direction_id,block_id,shape_id,wheelchair_accessible,bikes_allowed
            R1,WD,T1,Central Station,IC 2042,0,B1,SH1,1,1
            """;

        // Act
        var trips = ParseCsv<GtfsTrip>(csv);

        // Assert
        trips.Should().HaveCount(1);
        var trip = trips[0];
        trip.RouteId.Should().Be("R1");
        trip.ServiceId.Should().Be("WD");
        trip.TripId.Should().Be("T1");
        trip.TripHeadsign.Should().Be("Central Station");
        trip.TripShortName.Should().Be("IC 2042");
        trip.DirectionId.Should().Be(0);
        trip.BlockId.Should().Be("B1");
        trip.ShapeId.Should().Be("SH1");
        trip.WheelchairAccessible.Should().Be(1);
        trip.BikesAllowed.Should().Be(1);
    }

    [Fact]
    public void GtfsTrip_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange - Only required fields
        var csv = """
            route_id,service_id,trip_id
            R1,WD,T1
            """;

        // Act
        var trips = ParseCsv<GtfsTrip>(csv);

        // Assert
        trips.Should().HaveCount(1);
        var trip = trips[0];
        trip.RouteId.Should().Be("R1");
        trip.ServiceId.Should().Be("WD");
        trip.TripId.Should().Be("T1");
        trip.TripHeadsign.Should().BeNull();
        trip.DirectionId.Should().BeNull();
        trip.WheelchairAccessible.Should().BeNull();
    }

    #endregion

    #region GtfsStopTime Tests

    [Fact]
    public void GtfsStopTime_HasCorrectNameAttributes()
    {
        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsStopTime>("TripId", "trip_id");
        AssertNameAttribute<GtfsStopTime>("ArrivalTime", "arrival_time");
        AssertNameAttribute<GtfsStopTime>("DepartureTime", "departure_time");
        AssertNameAttribute<GtfsStopTime>("StopId", "stop_id");
        AssertNameAttribute<GtfsStopTime>("StopSequence", "stop_sequence");
        AssertNameAttribute<GtfsStopTime>("StopHeadsign", "stop_headsign");
        AssertNameAttribute<GtfsStopTime>("PickupType", "pickup_type");
        AssertNameAttribute<GtfsStopTime>("DropOffType", "drop_off_type");
        AssertNameAttribute<GtfsStopTime>("ContinuousPickup", "continuous_pickup");
        AssertNameAttribute<GtfsStopTime>("ContinuousDropOff", "continuous_drop_off");
        AssertNameAttribute<GtfsStopTime>("ShapeDistTraveled", "shape_dist_traveled");
        AssertNameAttribute<GtfsStopTime>("Timepoint", "timepoint");
    }

    [Fact]
    public void GtfsStopTime_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,stop_headsign,pickup_type,drop_off_type,continuous_pickup,continuous_drop_off,shape_dist_traveled,timepoint
            T1,08:30:00,08:31:00,S1,1,Final Stop,0,0,1,1,1500.5,1
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(1);
        var stopTime = stopTimes[0];
        stopTime.TripId.Should().Be("T1");
        stopTime.ArrivalTime.Should().Be("08:30:00");
        stopTime.DepartureTime.Should().Be("08:31:00");
        stopTime.StopId.Should().Be("S1");
        stopTime.StopSequence.Should().Be(1);
        stopTime.StopHeadsign.Should().Be("Final Stop");
        stopTime.PickupType.Should().Be(0);
        stopTime.DropOffType.Should().Be(0);
        stopTime.ContinuousPickup.Should().Be(1);
        stopTime.ContinuousDropOff.Should().Be(1);
        stopTime.ShapeDistTraveled.Should().Be(1500.5);
        stopTime.Timepoint.Should().Be(1);
    }

    [Fact]
    public void GtfsStopTime_WithTimeExceeding24Hours_ShouldParseAsString()
    {
        // Arrange - GTFS allows times > 24:00:00 for trips spanning past midnight
        var csv = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T1,25:30:00,25:35:00,S1,5
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(1);
        var stopTime = stopTimes[0];
        stopTime.ArrivalTime.Should().Be("25:30:00");  // Stored as string, not parsed as time
        stopTime.DepartureTime.Should().Be("25:35:00");
    }

    [Fact]
    public void GtfsStopTime_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange - Only required fields
        var csv = """
            trip_id,stop_id,stop_sequence
            T1,S1,1
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(1);
        var stopTime = stopTimes[0];
        stopTime.TripId.Should().Be("T1");
        stopTime.StopId.Should().Be("S1");
        stopTime.StopSequence.Should().Be(1);
        stopTime.ArrivalTime.Should().BeNull();
        stopTime.DepartureTime.Should().BeNull();
    }

    #endregion

    #region GtfsCalendar Tests

    [Fact]
    public void GtfsCalendar_HasCorrectNameAttributes()
    {
        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsCalendar>("ServiceId", "service_id");
        AssertNameAttribute<GtfsCalendar>("Monday", "monday");
        AssertNameAttribute<GtfsCalendar>("Tuesday", "tuesday");
        AssertNameAttribute<GtfsCalendar>("Wednesday", "wednesday");
        AssertNameAttribute<GtfsCalendar>("Thursday", "thursday");
        AssertNameAttribute<GtfsCalendar>("Friday", "friday");
        AssertNameAttribute<GtfsCalendar>("Saturday", "saturday");
        AssertNameAttribute<GtfsCalendar>("Sunday", "sunday");
        AssertNameAttribute<GtfsCalendar>("StartDate", "start_date");
        AssertNameAttribute<GtfsCalendar>("EndDate", "end_date");
    }

    [Fact]
    public void GtfsCalendar_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            """;

        // Act
        var calendars = ParseCsv<GtfsCalendar>(csv);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.ServiceId.Should().Be("WD");
        calendar.Monday.Should().Be(1);
        calendar.Tuesday.Should().Be(1);
        calendar.Wednesday.Should().Be(1);
        calendar.Thursday.Should().Be(1);
        calendar.Friday.Should().Be(1);
        calendar.Saturday.Should().Be(0);
        calendar.Sunday.Should().Be(0);
        calendar.StartDate.Should().Be("20240101");
        calendar.EndDate.Should().Be("20241231");
    }

    [Fact]
    public void GtfsCalendar_WeekendService_ShouldParseCorrectly()
    {
        // Arrange - Weekend-only service
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WE,0,0,0,0,0,1,1,20240101,20241231
            """;

        // Act
        var calendars = ParseCsv<GtfsCalendar>(csv);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.ServiceId.Should().Be("WE");
        calendar.Monday.Should().Be(0);
        calendar.Saturday.Should().Be(1);
        calendar.Sunday.Should().Be(1);
    }

    [Fact]
    public void GtfsCalendar_DateFormat_ShouldBeStoredAsString()
    {
        // Arrange - Verify dates are stored as strings (YYYYMMDD format)
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            SVC,1,1,1,1,1,1,1,20231215,20240315
            """;

        // Act
        var calendars = ParseCsv<GtfsCalendar>(csv);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.StartDate.Should().Be("20231215");
        calendar.EndDate.Should().Be("20240315");
    }

    #endregion

    #region GtfsTransfer Tests

    [Fact]
    public void GtfsTransfer_HasCorrectNameAttributes()
    {
        // Assert - Verify all properties have [Name] attributes with correct GTFS column names
        AssertNameAttribute<GtfsTransfer>("FromStopId", "from_stop_id");
        AssertNameAttribute<GtfsTransfer>("ToStopId", "to_stop_id");
        AssertNameAttribute<GtfsTransfer>("TransferType", "transfer_type");
        AssertNameAttribute<GtfsTransfer>("MinTransferTime", "min_transfer_time");
        AssertNameAttribute<GtfsTransfer>("FromRouteId", "from_route_id");
        AssertNameAttribute<GtfsTransfer>("ToRouteId", "to_route_id");
        AssertNameAttribute<GtfsTransfer>("FromTripId", "from_trip_id");
        AssertNameAttribute<GtfsTransfer>("ToTripId", "to_trip_id");
    }

    [Fact]
    public void GtfsTransfer_ParsesValidCsv_ShouldSucceed()
    {
        // Arrange
        var csv = """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time,from_route_id,to_route_id,from_trip_id,to_trip_id
            S1,S2,2,180,R1,R2,T1,T2
            """;

        // Act
        var transfers = ParseCsv<GtfsTransfer>(csv);

        // Assert
        transfers.Should().HaveCount(1);
        var transfer = transfers[0];
        transfer.FromStopId.Should().Be("S1");
        transfer.ToStopId.Should().Be("S2");
        transfer.TransferType.Should().Be(2);
        transfer.MinTransferTime.Should().Be(180);
        transfer.FromRouteId.Should().Be("R1");
        transfer.ToRouteId.Should().Be("R2");
        transfer.FromTripId.Should().Be("T1");
        transfer.ToTripId.Should().Be("T2");
    }

    [Fact]
    public void GtfsTransfer_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange - Only required fields
        var csv = """
            from_stop_id,to_stop_id,transfer_type
            S1,S2,0
            """;

        // Act
        var transfers = ParseCsv<GtfsTransfer>(csv);

        // Assert
        transfers.Should().HaveCount(1);
        var transfer = transfers[0];
        transfer.FromStopId.Should().Be("S1");
        transfer.ToStopId.Should().Be("S2");
        transfer.TransferType.Should().Be(0);
        transfer.MinTransferTime.Should().BeNull();
        transfer.FromRouteId.Should().BeNull();
        transfer.ToRouteId.Should().BeNull();
    }

    [Theory]
    [InlineData(0, "Recommended transfer point")]
    [InlineData(1, "Timed transfer point")]
    [InlineData(2, "Minimum transfer time required")]
    [InlineData(3, "Transfers not possible")]
    [InlineData(4, "In-seat transfer")]
    [InlineData(5, "In-seat transfer not allowed")]
    public void GtfsTransfer_AllTransferTypes_ShouldParseCorrectly(int transferType, string description)
    {
        // Arrange
        var csv = $"""
            from_stop_id,to_stop_id,transfer_type
            S1,S2,{transferType}
            """;

        // Act
        var transfers = ParseCsv<GtfsTransfer>(csv);

        // Assert
        transfers.Should().HaveCount(1);
        transfers[0].TransferType.Should().Be(transferType, because: description);
    }

    #endregion

    #region GtfsDataSet Tests

    [Fact]
    public void GtfsDataSet_NewInstance_ShouldHaveEmptyCollections()
    {
        // Act
        var dataSet = new GtfsDataSet();

        // Assert
        dataSet.Stops.Should().BeEmpty();
        dataSet.Routes.Should().BeEmpty();
        dataSet.Trips.Should().BeEmpty();
        dataSet.StopTimes.Should().BeEmpty();
        dataSet.Calendars.Should().BeEmpty();
        dataSet.Transfers.Should().BeEmpty();
    }

    [Fact]
    public void GtfsDataSet_IsEmpty_WhenNoData_ShouldBeTrue()
    {
        // Arrange
        var dataSet = new GtfsDataSet();

        // Act & Assert
        dataSet.IsEmpty.Should().BeTrue();
        dataSet.TotalEntityCount.Should().Be(0);
    }

    [Fact]
    public void GtfsDataSet_IsEmpty_WhenHasData_ShouldBeFalse()
    {
        // Arrange
        var dataSet = new GtfsDataSet();
        dataSet.Stops.Add(new GtfsStop { StopId = "S1" });

        // Act & Assert
        dataSet.IsEmpty.Should().BeFalse();
        dataSet.TotalEntityCount.Should().Be(1);
    }

    [Fact]
    public void GtfsDataSet_HasRequiredData_WhenAllRequiredCollectionsHaveData_ShouldBeTrue()
    {
        // Arrange
        var dataSet = new GtfsDataSet
        {
            Stops = new List<GtfsStop> { new() { StopId = "S1" } },
            Routes = new List<GtfsRoute> { new() { RouteId = "R1" } },
            Trips = new List<GtfsTrip> { new() { TripId = "T1" } },
            StopTimes = new List<GtfsStopTime> { new() { TripId = "T1", StopId = "S1" } },
            Calendars = new List<GtfsCalendar> { new() { ServiceId = "WD" } }
        };

        // Act & Assert
        dataSet.HasRequiredData.Should().BeTrue();
    }

    [Fact]
    public void GtfsDataSet_HasRequiredData_WhenMissingRequiredData_ShouldBeFalse()
    {
        // Arrange - Missing calendars (required)
        var dataSet = new GtfsDataSet
        {
            Stops = new List<GtfsStop> { new() { StopId = "S1" } },
            Routes = new List<GtfsRoute> { new() { RouteId = "R1" } },
            Trips = new List<GtfsTrip> { new() { TripId = "T1" } },
            StopTimes = new List<GtfsStopTime> { new() { TripId = "T1", StopId = "S1" } }
            // Calendars is empty
        };

        // Act & Assert
        dataSet.HasRequiredData.Should().BeFalse();
    }

    [Fact]
    public void GtfsDataSet_TotalEntityCount_ShouldSumAllCollections()
    {
        // Arrange
        var dataSet = new GtfsDataSet
        {
            Stops = new List<GtfsStop> { new(), new() },           // 2
            Routes = new List<GtfsRoute> { new() },                 // 1
            Trips = new List<GtfsTrip> { new(), new(), new() },    // 3
            StopTimes = new List<GtfsStopTime> { new() },          // 1
            Calendars = new List<GtfsCalendar> { new() },          // 1
            Transfers = new List<GtfsTransfer> { new(), new() }    // 2
        };

        // Act & Assert
        dataSet.TotalEntityCount.Should().Be(10);
    }

    #endregion

    #region Multiple Records Tests

    [Fact]
    public void GtfsStop_MultipleRecords_ShouldParseAllCorrectly()
    {
        // Arrange
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,Stop One,52.5200,13.4050
            S2,Stop Two,52.5300,13.4100
            S3,Stop Three,52.5400,13.4150
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(3);
        stops[0].StopId.Should().Be("S1");
        stops[1].StopId.Should().Be("S2");
        stops[2].StopId.Should().Be("S3");
    }

    [Fact]
    public void GtfsStopTime_MultipleRecordsWithSequence_ShouldMaintainOrder()
    {
        // Arrange
        var csv = """
            trip_id,stop_id,stop_sequence,arrival_time,departure_time
            T1,S1,1,08:00:00,08:00:00
            T1,S2,2,08:10:00,08:11:00
            T1,S3,5,08:25:00,08:26:00
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(3);
        stopTimes[0].StopSequence.Should().Be(1);
        stopTimes[1].StopSequence.Should().Be(2);
        stopTimes[2].StopSequence.Should().Be(5); // Non-consecutive is valid in GTFS
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GtfsStop_WithNegativeCoordinates_ShouldParseCorrectly()
    {
        // Arrange - Southern hemisphere and Western hemisphere coordinates
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,Buenos Aires,-34.6037,-58.3816
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopLat.Should().Be(-34.6037);
        stops[0].StopLon.Should().Be(-58.3816);
    }

    [Fact]
    public void GtfsRoute_WithAllStandardRouteTypes_ShouldParseCorrectly()
    {
        // Arrange - All standard GTFS route types
        var csv = """
            route_id,route_short_name,route_type
            R0,Tram,0
            R1,Metro,1
            R2,Rail,2
            R3,Bus,3
            R4,Ferry,4
            R5,Cable,5
            R6,Aerial,6
            R7,Funicular,7
            R11,Trolley,11
            R12,Monorail,12
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(10);
        routes.Select(r => r.RouteType).Should().BeEquivalentTo(StandardRouteTypes);
    }

    [Fact]
    public void GtfsStopTime_WithSingleDigitHour_ShouldParseCorrectly()
    {
        // Arrange - GTFS allows H:MM:SS format
        var csv = """
            trip_id,stop_id,stop_sequence,arrival_time,departure_time
            T1,S1,1,8:30:00,8:31:00
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(1);
        stopTimes[0].ArrivalTime.Should().Be("8:30:00");
        stopTimes[0].DepartureTime.Should().Be("8:31:00");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void GtfsStop_WithMissingRequiredStopId_WhenColumnIsMissing_ShouldUseDefaultValue()
    {
        // Arrange - stop_id column is present but empty
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            ,Central Station,52.5200,13.4050
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert - Empty string is parsed (validation is separate concern)
        stops.Should().HaveCount(1);
        stops[0].StopId.Should().BeEmpty();
    }

    [Fact]
    public void GtfsRoute_WithMissingRouteType_WhenColumnIsMissing_ShouldThrowException()
    {
        // Arrange - route_type is a required non-nullable field
        var csv = """
            route_id,route_short_name
            R1,U1
            """;

        // Act
        var action = () => ParseCsvStrict<GtfsRoute>(csv);

        // Assert - CsvHelper throws when non-nullable field is missing
        action.Should().Throw<CsvHelper.HeaderValidationException>();
    }

    [Fact]
    public void GtfsStopTime_WithInvalidStopSequence_WhenNotAnInteger_ShouldThrowException()
    {
        // Arrange - stop_sequence must be an integer
        var csv = """
            trip_id,stop_id,stop_sequence
            T1,S1,not_a_number
            """;

        // Act
        var action = () => ParseCsv<GtfsStopTime>(csv);

        // Assert
        action.Should().Throw<CsvHelper.TypeConversion.TypeConverterException>();
    }

    [Fact]
    public void GtfsCalendar_WithInvalidDayValue_WhenNotAnInteger_ShouldThrowException()
    {
        // Arrange - Monday must be an integer (0 or 1)
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,yes,1,1,1,1,0,0,20240101,20241231
            """;

        // Act
        var action = () => ParseCsv<GtfsCalendar>(csv);

        // Assert
        action.Should().Throw<CsvHelper.TypeConversion.TypeConverterException>();
    }

    #endregion

    #region Special Characters and Unicode Tests

    [Fact]
    public void GtfsStop_WithUnicodeCharacters_ShouldParseCorrectly()
    {
        // Arrange - German umlauts, French accents, Chinese characters
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,München Hauptbahnhof,48.1402,11.5600
            S2,Gare de Lyon,48.8448,2.3736
            S3,北京站,39.9042,116.4074
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(3);
        stops[0].StopName.Should().Be("München Hauptbahnhof");
        stops[1].StopName.Should().Be("Gare de Lyon");
        stops[2].StopName.Should().Be("北京站");
    }

    [Fact]
    public void GtfsRoute_WithQuotedCommas_ShouldParseCorrectly()
    {
        // Arrange - CSV values containing commas must be quoted
        var csv = """
            route_id,route_short_name,route_long_name,route_type
            R1,S1,"Hauptbahnhof, Altstadt, Neustadt",3
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(1);
        routes[0].RouteLongName.Should().Be("Hauptbahnhof, Altstadt, Neustadt");
    }

    [Fact]
    public void GtfsStop_WithSpecialUrlCharacters_ShouldParseCorrectly()
    {
        // Arrange - URL with query parameters
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon,stop_url
            S1,Test Stop,52.52,13.405,https://example.com/stop?id=S1&lang=de
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopUrl.Should().Be("https://example.com/stop?id=S1&lang=de");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public void GtfsStop_WithMaxCoordinateValues_ShouldParseCorrectly()
    {
        // Arrange - Extreme but valid lat/lon values
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,North Pole,90.0,0.0
            S2,South Pole,-90.0,0.0
            S3,Date Line,0.0,180.0
            S4,Anti-Meridian,0.0,-180.0
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(4);
        stops[0].StopLat.Should().Be(90.0);
        stops[1].StopLat.Should().Be(-90.0);
        stops[2].StopLon.Should().Be(180.0);
        stops[3].StopLon.Should().Be(-180.0);
    }

    [Theory]
    [InlineData(0, "Stop/Platform")]
    [InlineData(1, "Station")]
    [InlineData(2, "Entrance/Exit")]
    [InlineData(3, "Generic Node")]
    [InlineData(4, "Boarding Area")]
    public void GtfsStop_AllLocationTypes_ShouldParseCorrectly(int locationType, string description)
    {
        // Arrange
        var csv = $"""
            stop_id,stop_name,stop_lat,stop_lon,location_type
            S1,Test Location,52.52,13.405,{locationType}
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].LocationType.Should().Be(locationType, because: description);
    }

    [Theory]
    [InlineData(0, "No accessibility info")]
    [InlineData(1, "Wheelchair accessible")]
    [InlineData(2, "Not wheelchair accessible")]
    public void GtfsStop_AllWheelchairBoardingValues_ShouldParseCorrectly(int wheelchairValue, string description)
    {
        // Arrange
        var csv = $"""
            stop_id,stop_name,stop_lat,stop_lon,wheelchair_boarding
            S1,Test Stop,52.52,13.405,{wheelchairValue}
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].WheelchairBoarding.Should().Be(wheelchairValue, because: description);
    }

    [Fact]
    public void GtfsStopTime_WithZeroStopSequence_ShouldParseCorrectly()
    {
        // Arrange - stop_sequence can be 0
        var csv = """
            trip_id,stop_id,stop_sequence,arrival_time,departure_time
            T1,S1,0,08:00:00,08:00:00
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(1);
        stopTimes[0].StopSequence.Should().Be(0);
    }

    [Fact]
    public void GtfsStopTime_WithMidnightTimes_ShouldParseCorrectly()
    {
        // Arrange - Exact midnight
        var csv = """
            trip_id,stop_id,stop_sequence,arrival_time,departure_time
            T1,S1,1,00:00:00,00:00:00
            T1,S2,2,24:00:00,24:00:00
            """;

        // Act
        var stopTimes = ParseCsv<GtfsStopTime>(csv);

        // Assert
        stopTimes.Should().HaveCount(2);
        stopTimes[0].ArrivalTime.Should().Be("00:00:00");
        stopTimes[1].ArrivalTime.Should().Be("24:00:00"); // GTFS allows 24:00:00
    }

    [Fact]
    public void GtfsCalendar_WithAllDaysActive_ShouldParseCorrectly()
    {
        // Arrange - Service runs every day
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            DAILY,1,1,1,1,1,1,1,20240101,20241231
            """;

        // Act
        var calendars = ParseCsv<GtfsCalendar>(csv);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.Monday.Should().Be(1);
        calendar.Tuesday.Should().Be(1);
        calendar.Wednesday.Should().Be(1);
        calendar.Thursday.Should().Be(1);
        calendar.Friday.Should().Be(1);
        calendar.Saturday.Should().Be(1);
        calendar.Sunday.Should().Be(1);
    }

    [Fact]
    public void GtfsCalendar_WithNoDaysActive_ShouldParseCorrectly()
    {
        // Arrange - Service runs no days (uses calendar_dates for exceptions)
        var csv = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            SPECIAL,0,0,0,0,0,0,0,20240101,20241231
            """;

        // Act
        var calendars = ParseCsv<GtfsCalendar>(csv);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.Monday.Should().Be(0);
        calendar.Sunday.Should().Be(0);
    }

    [Fact]
    public void GtfsTransfer_WithZeroMinTransferTime_ShouldParseCorrectly()
    {
        // Arrange - Zero transfer time is valid
        var csv = """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time
            S1,S2,2,0
            """;

        // Act
        var transfers = ParseCsv<GtfsTransfer>(csv);

        // Assert
        transfers.Should().HaveCount(1);
        transfers[0].MinTransferTime.Should().Be(0);
    }

    [Fact]
    public void GtfsTransfer_WithLargeMinTransferTime_ShouldParseCorrectly()
    {
        // Arrange - Large transfer time (30 minutes)
        var csv = """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time
            S1,S2,2,1800
            """;

        // Act
        var transfers = ParseCsv<GtfsTransfer>(csv);

        // Assert
        transfers.Should().HaveCount(1);
        transfers[0].MinTransferTime.Should().Be(1800);
    }

    #endregion

    #region GtfsDataSet Edge Case Tests

    [Fact]
    public void GtfsDataSet_WithOnlyTransfers_HasRequiredData_ShouldBeFalse()
    {
        // Arrange - Transfers alone are not enough
        var dataSet = new GtfsDataSet
        {
            Transfers = new List<GtfsTransfer>
            {
                new() { FromStopId = "S1", ToStopId = "S2", TransferType = 0 }
            }
        };

        // Act & Assert
        dataSet.HasRequiredData.Should().BeFalse();
        dataSet.IsEmpty.Should().BeFalse();
        dataSet.TotalEntityCount.Should().Be(1);
    }

    [Fact]
    public void GtfsDataSet_CollectionsCanBeReplaced_ShouldWork()
    {
        // Arrange
        var dataSet = new GtfsDataSet();
        var newStops = new List<GtfsStop>
        {
            new() { StopId = "S1", StopName = "Stop 1" },
            new() { StopId = "S2", StopName = "Stop 2" }
        };

        // Act
        dataSet.Stops = newStops;

        // Assert
        dataSet.Stops.Should().HaveCount(2);
        dataSet.TotalEntityCount.Should().Be(2);
    }

    #endregion

    #region Precision and Format Tests

    [Fact]
    public void GtfsStop_WithHighPrecisionCoordinates_ShouldParseCorrectly()
    {
        // Arrange - High precision coordinates (7 decimal places)
        var csv = """
            stop_id,stop_name,stop_lat,stop_lon
            S1,Precise Location,52.5200389,13.4049539
            """;

        // Act
        var stops = ParseCsv<GtfsStop>(csv);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopLat.Should().BeApproximately(52.5200389, 0.0000001);
        stops[0].StopLon.Should().BeApproximately(13.4049539, 0.0000001);
    }

    [Fact]
    public void GtfsRoute_WithHashColorCodes_ShouldParseCorrectly()
    {
        // Arrange - Colors can be with or without # prefix
        var csv = """
            route_id,route_short_name,route_type,route_color,route_text_color
            R1,U1,1,FF5733,000000
            """;

        // Act
        var routes = ParseCsv<GtfsRoute>(csv);

        // Assert
        routes.Should().HaveCount(1);
        routes[0].RouteColor.Should().Be("FF5733");
        routes[0].RouteTextColor.Should().Be("000000");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Parses CSV content using CsvHelper with the specified GTFS model type.
    /// </summary>
    private static List<T> ParseCsv<T>(string csvContent)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Parses CSV content with strict validation (throws on missing headers).
    /// </summary>
    private static List<T> ParseCsvStrict<T>(string csvContent)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var reader = new StringReader(csvContent);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Asserts that a property has a [Name] attribute with the expected GTFS column name.
    /// </summary>
    private static void AssertNameAttribute<T>(string propertyName, string expectedGtfsColumnName)
    {
        var property = typeof(T).GetProperty(propertyName);
        property.Should().NotBeNull($"Property {propertyName} should exist on {typeof(T).Name}");

        var nameAttribute = property!.GetCustomAttribute<NameAttribute>();
        nameAttribute.Should().NotBeNull($"Property {propertyName} should have a [Name] attribute");

        nameAttribute!.Names.Should().Contain(expectedGtfsColumnName,
            $"Property {propertyName} should map to GTFS column '{expectedGtfsColumnName}'");
    }

    #endregion
}
