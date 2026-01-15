using System.Globalization;
using System.Text;
using Fahrplanauskunft.Infrastructure.Gtfs;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fahrplanauskunft.Tests.Integration.Gtfs;

/// <summary>
/// Integration tests for the <see cref="GtfsParser"/> class.
/// These tests use sample GTFS files stored on disk to verify the complete parsing workflow.
/// </summary>
public class GtfsParserIntegrationTests : IDisposable
{
    private readonly ILogger<GtfsParser> _logger;
    private readonly GtfsParser _parser;
    private readonly string _gtfsDirectory;

    public GtfsParserIntegrationTests()
    {
        _logger = Substitute.For<ILogger<GtfsParser>>();
        _parser = new GtfsParser(_logger);
        _gtfsDirectory = Path.Combine(Path.GetTempPath(), $"GtfsIntegrationTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_gtfsDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_gtfsDirectory))
        {
            Directory.Delete(_gtfsDirectory, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    #region Sample GTFS Data - Realistic Hamburg Transit Network

    /// <summary>
    /// Creates a complete sample GTFS data set representing a simplified Hamburg transit network.
    /// This includes multiple stops, routes, trips, calendars, stop times, and transfers.
    /// </summary>
    private async Task CreateSampleGtfsDataSet()
    {
        // stops.txt - Multiple stations with various properties
        await CreateFile("stops.txt", """
            stop_id,stop_code,stop_name,stop_desc,stop_lat,stop_lon,zone_id,location_type,parent_station,wheelchair_boarding
            de:09000:1,HBF,Hamburg Hauptbahnhof,Central train station,53.552736,10.006909,A,1,,1
            de:09000:1:1,HBF-U,Hamburg Hbf U-Bahn,Underground platform,53.552800,10.007100,A,0,de:09000:1,1
            de:09000:1:2,HBF-S,Hamburg Hbf S-Bahn,Suburban rail platform,53.552650,10.006800,A,0,de:09000:1,1
            de:09000:2,JUN,Jungfernstieg,City center interchange,53.551401,9.993242,A,1,,1
            de:09000:2:1,JUN-U,Jungfernstieg U-Bahn,Underground platform,53.551500,9.993400,A,0,de:09000:2,1
            de:09000:3,DAM,Dammtor,Science district station,53.560970,9.989670,A,0,,1
            de:09000:4,ALT,Altona,Western terminus,53.552670,9.935280,B,1,,1
            de:09000:4:1,ALT-S,Altona S-Bahn,S-Bahn platform,53.552700,9.935400,B,0,de:09000:4,1
            de:09000:5,LAND,Landungsbrücken,Harbor district,53.546170,9.969310,A,0,,1
            de:09000:6,BERL,Berliner Tor,Eastern interchange,53.553350,10.024060,A,1,,1
            de:09000:6:1,BERL-U,Berliner Tor U-Bahn,Underground platform,53.553400,10.024200,A,0,de:09000:6,1
            """);

        // routes.txt - Various transit types
        await CreateFile("routes.txt", """
            route_id,agency_id,route_short_name,route_long_name,route_desc,route_type,route_url,route_color,route_text_color,route_sort_order
            U1,HVV,U1,U-Bahn Linie 1,Norderstedt - Ohlstedt/Großhansdorf,1,,0069B4,FFFFFF,100
            U2,HVV,U2,U-Bahn Linie 2,Niendorf Nord - Mümmelmannsberg,1,,DA291C,FFFFFF,101
            U3,HVV,U3,U-Bahn Linie 3,Barmbek - Wandsbek-Gartenstadt,1,,FFCC00,000000,102
            S1,HVV,S1,S-Bahn Linie 1,Wedel/Poppenbüttel/Flughafen - Ohlsdorf,2,,009B3A,FFFFFF,200
            S3,HVV,S3,S-Bahn Linie 3,Pinneberg - Neugraben/Stade,2,,522398,FFFFFF,202
            BUS5,HVV,5,Metrobus Linie 5,Hauptbahnhof - Niendorf Markt,3,,E30613,FFFFFF,300
            FERRY62,HVV,62,Hafenfähre 62,Landungsbrücken - Finkenwerder,4,,0069B4,FFFFFF,400
            """);

        // trips.txt - Trips with various properties
        await CreateFile("trips.txt", """
            route_id,service_id,trip_id,trip_headsign,trip_short_name,direction_id,block_id,shape_id,wheelchair_accessible,bikes_allowed
            U1,WD,U1_WD_001,Norderstedt Mitte,U1-001,0,BLK_U1_1,SHP_U1_OUT,1,1
            U1,WD,U1_WD_002,Ohlstedt,U1-002,1,BLK_U1_2,SHP_U1_IN,1,1
            U1,WE,U1_WE_001,Norderstedt Mitte,U1-003,0,BLK_U1_3,SHP_U1_OUT,1,1
            U2,WD,U2_WD_001,Mümmelmannsberg,U2-001,0,BLK_U2_1,SHP_U2_OUT,1,0
            U3,WD,U3_WD_001,Wandsbek-Gartenstadt,U3-001,0,BLK_U3_1,SHP_U3_OUT,1,1
            S1,WD,S1_WD_001,Flughafen,S1-001,0,BLK_S1_1,SHP_S1_OUT,1,1
            S1,WD,S1_WD_002,Wedel,S1-002,1,BLK_S1_2,SHP_S1_IN,1,1
            S3,WD,S3_WD_001,Stade,S3-001,0,BLK_S3_1,SHP_S3_OUT,1,1
            BUS5,WD,BUS5_WD_001,Niendorf Markt,B5-001,0,BLK_B5_1,,1,0
            FERRY62,WD,F62_WD_001,Finkenwerder,F62-001,0,,,1,1
            U1,NIGHT,U1_NT_001,Norderstedt Mitte,U1-N01,0,BLK_U1_N1,SHP_U1_OUT,1,0
            """);

        // calendar.txt - Service patterns
        await CreateFile("calendar.txt", """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            WE,0,0,0,0,0,1,1,20240101,20241231
            DAILY,1,1,1,1,1,1,1,20240101,20241231
            NIGHT,1,1,1,1,1,1,1,20240101,20241231
            HOLIDAY,0,0,0,0,0,0,0,20240101,20241231
            """);

        // stop_times.txt - Comprehensive stop times with extended time formats for overnight trips
        await CreateFile("stop_times.txt", """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,stop_headsign,pickup_type,drop_off_type,shape_dist_traveled,timepoint
            U1_WD_001,06:00:00,06:00:00,de:09000:1:1,1,,0,0,0.0,1
            U1_WD_001,06:03:00,06:04:00,de:09000:2:1,2,,0,0,1.2,1
            U1_WD_001,06:08:00,06:08:00,de:09000:3,3,Dammtor,0,0,2.5,1
            U1_WD_002,06:30:00,06:30:00,de:09000:3,1,,0,0,0.0,1
            U1_WD_002,06:34:00,06:35:00,de:09000:2:1,2,,0,0,1.2,1
            U1_WD_002,06:38:00,06:38:00,de:09000:1:1,3,Hauptbahnhof,0,0,2.5,1
            S1_WD_001,07:00:00,07:00:00,de:09000:4:1,1,,0,0,0.0,1
            S1_WD_001,07:08:00,07:09:00,de:09000:1:2,2,,0,0,5.5,1
            S1_WD_001,07:15:00,07:15:00,de:09000:6:1,3,Berliner Tor,0,0,8.2,1
            U1_NT_001,23:30:00,23:30:00,de:09000:1:1,1,,0,0,0.0,1
            U1_NT_001,23:33:00,23:34:00,de:09000:2:1,2,,0,0,1.2,1
            U1_NT_001,23:38:00,23:39:00,de:09000:3,3,,0,0,2.5,1
            U1_NT_001,24:00:00,24:01:00,de:09000:1:1,4,,0,0,5.0,1
            U1_NT_001,24:30:00,24:31:00,de:09000:2:1,5,,0,0,6.2,1
            U1_NT_001,25:00:00,25:00:00,de:09000:3,6,End Station,0,0,7.5,1
            """);

        // transfers.txt - Transfer connections between lines
        await CreateFile("transfers.txt", """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time,from_route_id,to_route_id,from_trip_id,to_trip_id
            de:09000:1:1,de:09000:1:2,2,180,U1,S1,,
            de:09000:1:2,de:09000:1:1,2,180,S1,U1,,
            de:09000:2:1,de:09000:5,2,300,U3,FERRY62,,
            de:09000:4:1,de:09000:4:1,1,0,S1,S3,,
            de:09000:6:1,de:09000:6:1,0,,,,,
            """);
    }

    /// <summary>
    /// Creates sample GTFS files without transfers.txt to test optional file handling.
    /// </summary>
    private async Task CreateSampleGtfsWithoutTransfers()
    {
        await CreateFile("stops.txt", """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Hamburg Hbf,53.552736,10.006909
            S002,Jungfernstieg,53.551401,9.993242
            S003,Dammtor,53.560970,9.989670
            """);

        await CreateFile("routes.txt", """
            route_id,route_short_name,route_type
            U1,U1,1
            S1,S1,2
            """);

        await CreateFile("trips.txt", """
            route_id,service_id,trip_id
            U1,WD,T001
            S1,WD,T002
            """);

        await CreateFile("calendar.txt", """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            """);

        await CreateFile("stop_times.txt", """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,1
            T001,08:05:00,08:05:00,S002,2
            T002,08:30:00,08:30:00,S001,1
            T002,08:38:00,08:38:00,S003,2
            """);
    }

    /// <summary>
    /// Creates sample GTFS files with BOM encoding.
    /// </summary>
    private async Task CreateSampleGtfsWithBom()
    {
        var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        await File.WriteAllTextAsync(
            Path.Combine(_gtfsDirectory, "stops.txt"),
            """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Hamburg Hbf,53.552736,10.006909
            S002,Jungfernstieg,53.551401,9.993242
            """,
            utf8WithBom);

        await File.WriteAllTextAsync(
            Path.Combine(_gtfsDirectory, "routes.txt"),
            """
            route_id,route_short_name,route_type
            R001,U1,1
            """,
            utf8WithBom);

        await File.WriteAllTextAsync(
            Path.Combine(_gtfsDirectory, "trips.txt"),
            """
            route_id,service_id,trip_id
            R001,WD,T001
            """,
            utf8WithBom);

        await File.WriteAllTextAsync(
            Path.Combine(_gtfsDirectory, "calendar.txt"),
            """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            """,
            utf8WithBom);

        await File.WriteAllTextAsync(
            Path.Combine(_gtfsDirectory, "stop_times.txt"),
            """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,1
            T001,08:05:00,08:05:00,S002,2
            """,
            utf8WithBom);
    }

    /// <summary>
    /// Creates a larger sample GTFS data set for performance testing.
    /// </summary>
    private async Task CreateLargeGtfsDataSet(int stopCount, int tripCount, int stopTimesPerTrip)
    {
        // Create stops
        var stopsContent = new StringBuilder("stop_id,stop_name,stop_lat,stop_lon\n");
        for (int i = 1; i <= stopCount; i++)
        {
            stopsContent.AppendLine(CultureInfo.InvariantCulture, $"S{i:D4},Stop {i},{53.5 + (i * 0.001):F6},{9.9 + (i * 0.001):F6}");
        }
        await CreateFile("stops.txt", stopsContent.ToString());

        // Create routes
        await CreateFile("routes.txt", """
            route_id,route_short_name,route_type
            R001,Line 1,1
            R002,Line 2,2
            """);

        // Create trips
        var tripsContent = new StringBuilder("route_id,service_id,trip_id\n");
        for (int i = 1; i <= tripCount; i++)
        {
            var routeId = i % 2 == 0 ? "R002" : "R001";
            tripsContent.AppendLine(CultureInfo.InvariantCulture, $"{routeId},WD,T{i:D4}");
        }
        await CreateFile("trips.txt", tripsContent.ToString());

        // Create calendar
        await CreateFile("calendar.txt", """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            """);

        // Create stop_times - This will be the largest file
        var stopTimesContent = new StringBuilder("trip_id,arrival_time,departure_time,stop_id,stop_sequence\n");
        for (int tripNum = 1; tripNum <= tripCount; tripNum++)
        {
            for (int seq = 1; seq <= stopTimesPerTrip; seq++)
            {
                var hour = 6 + ((tripNum * stopTimesPerTrip + seq) / 60) % 24;
                var minute = seq % 60;
                var stopId = $"S{((tripNum + seq) % stopCount) + 1:D4}";
                stopTimesContent.AppendLine(CultureInfo.InvariantCulture, $"T{tripNum:D4},{hour:D2}:{minute:D2}:00,{hour:D2}:{minute:D2}:30,{stopId},{seq}");
            }
        }
        await CreateFile("stop_times.txt", stopTimesContent.ToString());
    }

    private async Task CreateFile(string fileName, string content)
    {
        var filePath = Path.Combine(_gtfsDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    #endregion

    #region Complete GTFS Data Set Parsing Tests

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseAllFiles()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Should().NotBeNull();
        dataSet.HasRequiredData.Should().BeTrue();
        dataSet.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectStopCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Stops.Should().HaveCount(11);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectRouteCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Routes.Should().HaveCount(7);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectTripCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Trips.Should().HaveCount(11);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectCalendarCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Calendars.Should().HaveCount(5);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectStopTimesCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.StopTimes.Should().HaveCount(15);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldParseCorrectTransfersCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Transfers.Should().HaveCount(5);
    }

    [Fact]
    public async Task ParseAsync_WithCompleteSampleGtfs_ShouldCalculateCorrectTotalEntityCount()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.TotalEntityCount.Should().Be(11 + 7 + 11 + 5 + 15 + 5); // stops + routes + trips + calendars + stop_times + transfers = 54
    }

    #endregion

    #region Stop Data Validation Tests

    [Fact]
    public async Task ParseAsync_ShouldParseStopProperties_Correctly()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var hauptbahnhof = dataSet.Stops.FirstOrDefault(s => s.StopId == "de:09000:1");
        hauptbahnhof.Should().NotBeNull();
        hauptbahnhof!.StopCode.Should().Be("HBF");
        hauptbahnhof.StopName.Should().Be("Hamburg Hauptbahnhof");
        hauptbahnhof.StopDesc.Should().Be("Central train station");
        hauptbahnhof.StopLat.Should().BeApproximately(53.552736, 0.000001);
        hauptbahnhof.StopLon.Should().BeApproximately(10.006909, 0.000001);
        hauptbahnhof.ZoneId.Should().Be("A");
        hauptbahnhof.LocationType.Should().Be(1); // Station
    }

    [Fact]
    public async Task ParseAsync_ShouldParseParentStationRelationships()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var childStop = dataSet.Stops.FirstOrDefault(s => s.StopId == "de:09000:1:1");
        childStop.Should().NotBeNull();
        childStop!.ParentStation.Should().Be("de:09000:1");
        childStop.LocationType.Should().Be(0); // Platform
    }

    [Fact]
    public async Task ParseAsync_ShouldParseDifferentZoneIds()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var zoneAStops = dataSet.Stops.Where(s => s.ZoneId == "A").ToList();
        var zoneBStops = dataSet.Stops.Where(s => s.ZoneId == "B").ToList();

        zoneAStops.Should().HaveCountGreaterThan(0);
        zoneBStops.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Route Data Validation Tests

    [Fact]
    public async Task ParseAsync_ShouldParseRouteProperties_Correctly()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var u1 = dataSet.Routes.FirstOrDefault(r => r.RouteId == "U1");
        u1.Should().NotBeNull();
        u1!.AgencyId.Should().Be("HVV");
        u1.RouteShortName.Should().Be("U1");
        u1.RouteLongName.Should().Be("U-Bahn Linie 1");
        u1.RouteType.Should().Be(1); // Subway
        u1.RouteColor.Should().Be("0069B4");
        u1.RouteTextColor.Should().Be("FFFFFF");
    }

    [Fact]
    public async Task ParseAsync_ShouldParseAllRouteTypes()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var routeTypes = dataSet.Routes.Select(r => r.RouteType).Distinct().ToList();
        routeTypes.Should().Contain(1); // Subway
        routeTypes.Should().Contain(2); // Rail
        routeTypes.Should().Contain(3); // Bus
        routeTypes.Should().Contain(4); // Ferry
    }

    #endregion

    #region Trip Data Validation Tests

    [Fact]
    public async Task ParseAsync_ShouldParseTripProperties_Correctly()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var trip = dataSet.Trips.FirstOrDefault(t => t.TripId == "U1_WD_001");
        trip.Should().NotBeNull();
        trip!.RouteId.Should().Be("U1");
        trip.ServiceId.Should().Be("WD");
        trip.TripHeadsign.Should().Be("Norderstedt Mitte");
        trip.TripShortName.Should().Be("U1-001");
        trip.DirectionId.Should().Be(0);
        trip.BlockId.Should().Be("BLK_U1_1");
        trip.ShapeId.Should().Be("SHP_U1_OUT");
        trip.WheelchairAccessible.Should().Be(1);
        trip.BikesAllowed.Should().Be(1);
    }

    [Fact]
    public async Task ParseAsync_ShouldParseBothDirections()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var direction0Trips = dataSet.Trips.Where(t => t.DirectionId == 0).ToList();
        var direction1Trips = dataSet.Trips.Where(t => t.DirectionId == 1).ToList();

        direction0Trips.Should().HaveCountGreaterThan(0);
        direction1Trips.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Calendar Data Validation Tests

    [Fact]
    public async Task ParseAsync_ShouldParseCalendarProperties_Correctly()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var weekday = dataSet.Calendars.FirstOrDefault(c => c.ServiceId == "WD");
        weekday.Should().NotBeNull();
        weekday!.Monday.Should().Be(1);
        weekday.Tuesday.Should().Be(1);
        weekday.Wednesday.Should().Be(1);
        weekday.Thursday.Should().Be(1);
        weekday.Friday.Should().Be(1);
        weekday.Saturday.Should().Be(0);
        weekday.Sunday.Should().Be(0);
        weekday.StartDate.Should().Be("20240101");
        weekday.EndDate.Should().Be("20241231");
    }

    [Fact]
    public async Task ParseAsync_ShouldParseWeekendCalendar()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var weekend = dataSet.Calendars.FirstOrDefault(c => c.ServiceId == "WE");
        weekend.Should().NotBeNull();
        weekend!.Monday.Should().Be(0);
        weekend.Saturday.Should().Be(1);
        weekend.Sunday.Should().Be(1);
    }

    #endregion

    #region Extended Time Format Tests

    [Fact]
    public async Task ParseAsync_ShouldPreserveExtendedTimeFormats()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var nightTripStopTimes = dataSet.StopTimes
            .Where(st => st.TripId == "U1_NT_001")
            .OrderBy(st => st.StopSequence)
            .ToList();

        nightTripStopTimes.Should().HaveCount(6);

        // Verify extended times are preserved as strings
        nightTripStopTimes[3].ArrivalTime.Should().Be("24:00:00"); // Midnight
        nightTripStopTimes[4].ArrivalTime.Should().Be("24:30:00"); // 00:30 next day
        nightTripStopTimes[5].ArrivalTime.Should().Be("25:00:00"); // 01:00 next day
    }

    [Fact]
    public async Task ParseAsync_ShouldPreserveTimeSequenceAcrossMidnight()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var nightTripStopTimes = dataSet.StopTimes
            .Where(st => st.TripId == "U1_NT_001")
            .OrderBy(st => st.StopSequence)
            .ToList();

        // Verify the chronological order is maintained when stored as strings
        var times = nightTripStopTimes.Select(st => st.ArrivalTime).ToList();
        times.Should().BeEquivalentTo(
            ["23:30:00", "23:33:00", "23:38:00", "24:00:00", "24:30:00", "25:00:00"],
            options => options.WithStrictOrdering());
    }

    #endregion

    #region Transfer Data Validation Tests

    [Fact]
    public async Task ParseAsync_ShouldParseTransferProperties_Correctly()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var transfer = dataSet.Transfers.FirstOrDefault(t =>
            t.FromStopId == "de:09000:1:1" && t.ToStopId == "de:09000:1:2");
        transfer.Should().NotBeNull();
        transfer!.TransferType.Should().Be(2); // Minimum transfer time required
        transfer.MinTransferTime.Should().Be(180); // 3 minutes
        transfer.FromRouteId.Should().Be("U1");
        transfer.ToRouteId.Should().Be("S1");
    }

    [Fact]
    public async Task ParseAsync_ShouldParseAllTransferTypes()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var transferTypes = dataSet.Transfers.Select(t => t.TransferType).Distinct().ToList();
        transferTypes.Should().Contain(0); // Recommended
        transferTypes.Should().Contain(1); // Timed
        transferTypes.Should().Contain(2); // Minimum time required
    }

    #endregion

    #region Optional File Handling Tests

    [Fact]
    public async Task ParseAsync_WithoutTransfersFile_ShouldReturnEmptyTransfersCollection()
    {
        // Arrange
        await CreateSampleGtfsWithoutTransfers();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Transfers.Should().BeEmpty();
        dataSet.HasRequiredData.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_WithoutTransfersFile_ShouldStillParseAllRequiredFiles()
    {
        // Arrange
        await CreateSampleGtfsWithoutTransfers();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Stops.Should().HaveCount(3);
        dataSet.Routes.Should().HaveCount(2);
        dataSet.Trips.Should().HaveCount(2);
        dataSet.Calendars.Should().HaveCount(1);
        dataSet.StopTimes.Should().HaveCount(4);
    }

    #endregion

    #region BOM Encoding Tests

    [Fact]
    public async Task ParseAsync_WithBomEncodedFiles_ShouldParseCorrectly()
    {
        // Arrange
        await CreateSampleGtfsWithBom();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Should().NotBeNull();
        dataSet.HasRequiredData.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_WithBomEncodedFiles_ShouldNotIncludeBomInData()
    {
        // Arrange
        await CreateSampleGtfsWithBom();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var firstStop = dataSet.Stops.First();
        firstStop.StopId.Should().NotStartWith("\uFEFF"); // BOM character
        firstStop.StopId.Should().Be("S001");

        var firstRoute = dataSet.Routes.First();
        firstRoute.RouteId.Should().NotStartWith("\uFEFF");
        firstRoute.RouteId.Should().Be("R001");
    }

    #endregion

    #region Progress Reporting Tests

    [Fact]
    public async Task ParseAsync_ShouldReportProgressForAllFiles()
    {
        // Arrange
        await CreateSampleGtfsDataSet();
        var progressReports = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

        // Act
        await _parser.ParseAsync(_gtfsDirectory, progress);

        // Allow time for async progress callbacks
        await Task.Delay(200);

        // Assert
        progressReports.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseAsync_ShouldReportProgressForEachFile()
    {
        // Arrange
        await CreateSampleGtfsDataSet();
        var reportedFiles = new HashSet<string>();
        var progress = new Progress<ImportProgress>(p => reportedFiles.Add(p.CurrentFile));

        // Act
        await _parser.ParseAsync(_gtfsDirectory, progress);

        // Allow time for async progress callbacks
        await Task.Delay(200);

        // Assert
        reportedFiles.Should().Contain("stops.txt");
        reportedFiles.Should().Contain("routes.txt");
        reportedFiles.Should().Contain("trips.txt");
        reportedFiles.Should().Contain("calendar.txt");
        reportedFiles.Should().Contain("stop_times.txt");
        reportedFiles.Should().Contain("transfers.txt");
    }

    [Fact]
    public async Task ParseAsync_ShouldReportCompletedStatus()
    {
        // Arrange
        await CreateSampleGtfsDataSet();
        var completedStatuses = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p =>
        {
            if (p.Status == "Completed")
                completedStatuses.Add(p);
        });

        // Act
        await _parser.ParseAsync(_gtfsDirectory, progress);

        // Allow time for async progress callbacks
        await Task.Delay(200);

        // Assert
        completedStatuses.Should().NotBeEmpty();
        completedStatuses.Should().Contain(p => p.CurrentFile == "stops.txt");
    }

    #endregion

    #region Performance Tests with Larger Data Sets

    [Fact]
    public async Task ParseAsync_WithLargerDataSet_ShouldParseAllRecords()
    {
        // Arrange - Create a moderately sized data set
        const int stopCount = 100;
        const int tripCount = 50;
        const int stopTimesPerTrip = 10;

        await CreateLargeGtfsDataSet(stopCount, tripCount, stopTimesPerTrip);

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Stops.Should().HaveCount(stopCount);
        dataSet.Trips.Should().HaveCount(tripCount);
        dataSet.StopTimes.Should().HaveCount(tripCount * stopTimesPerTrip);
    }

    [Fact]
    public async Task ParseAsync_WithLargerDataSet_ShouldReportProgressDuringParsing()
    {
        // Arrange
        const int stopCount = 100;
        const int tripCount = 50;
        const int stopTimesPerTrip = 10;

        await CreateLargeGtfsDataSet(stopCount, tripCount, stopTimesPerTrip);

        var progressReportCount = 0;
        var progress = new Progress<ImportProgress>(_ => Interlocked.Increment(ref progressReportCount));

        // Act
        await _parser.ParseAsync(_gtfsDirectory, progress);

        // Allow time for async progress callbacks
        await Task.Delay(200);

        // Assert
        progressReportCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ParseAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        await CreateSampleGtfsDataSet();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _parser.ParseAsync(_gtfsDirectory, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ParseAsync_WithNonExistentDirectory_ShouldThrowDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");

        // Act
        var act = async () => await _parser.ParseAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage($"*{nonExistentPath}*");
    }

    [Fact]
    public async Task ParseAsync_WithMissingRequiredFiles_ShouldThrowGtfsParsingException()
    {
        // Arrange - Directory exists but has no files

        // Act
        var act = async () => await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.IsMissingFilesError.Should().BeTrue();
        exception.Which.MissingFiles.Should().Contain("stops.txt");
        exception.Which.MissingFiles.Should().Contain("routes.txt");
        exception.Which.MissingFiles.Should().Contain("trips.txt");
        exception.Which.MissingFiles.Should().Contain("stop_times.txt");
        exception.Which.MissingFiles.Should().Contain("calendar.txt");
    }

    [Fact]
    public async Task ParseAsync_WithPartialRequiredFiles_ShouldThrowGtfsParsingExceptionForMissing()
    {
        // Arrange - Create only some required files
        await CreateFile("stops.txt", "stop_id,stop_name,stop_lat,stop_lon\nS001,Station,53.5,9.9");
        await CreateFile("routes.txt", "route_id,route_short_name,route_type\nR001,R1,1");
        // Missing: trips.txt, stop_times.txt, calendar.txt

        // Act
        var act = async () => await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.MissingFiles.Should().Contain("trips.txt");
        exception.Which.MissingFiles.Should().Contain("stop_times.txt");
        exception.Which.MissingFiles.Should().Contain("calendar.txt");
        exception.Which.MissingFiles.Should().NotContain("stops.txt");
        exception.Which.MissingFiles.Should().NotContain("routes.txt");
    }

    #endregion

    #region Relational Data Integrity Tests

    [Fact]
    public async Task ParseAsync_ShouldMaintainRouteToTripRelationship()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var routeIds = dataSet.Routes.Select(r => r.RouteId).ToHashSet();
        var tripRouteIds = dataSet.Trips.Select(t => t.RouteId).Distinct().ToList();

        // All trip route_ids should reference existing routes
        tripRouteIds.Should().OnlyContain(id => routeIds.Contains(id));
    }

    [Fact]
    public async Task ParseAsync_ShouldMaintainTripToStopTimeRelationship()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var tripIds = dataSet.Trips.Select(t => t.TripId).ToHashSet();
        var stopTimeTripIds = dataSet.StopTimes.Select(st => st.TripId).Distinct().ToList();

        // All stop_time trip_ids should reference existing trips
        stopTimeTripIds.Should().OnlyContain(id => tripIds.Contains(id));
    }

    [Fact]
    public async Task ParseAsync_ShouldMaintainStopTimeToStopRelationship()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var stopIds = dataSet.Stops.Select(s => s.StopId).ToHashSet();
        var stopTimeStopIds = dataSet.StopTimes.Select(st => st.StopId).Distinct().ToList();

        // All stop_time stop_ids should reference existing stops
        stopTimeStopIds.Should().OnlyContain(id => stopIds.Contains(id));
    }

    [Fact]
    public async Task ParseAsync_ShouldMaintainTripToCalendarRelationship()
    {
        // Arrange
        await CreateSampleGtfsDataSet();

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var serviceIds = dataSet.Calendars.Select(c => c.ServiceId).ToHashSet();
        var tripServiceIds = dataSet.Trips.Select(t => t.ServiceId).Distinct().ToList();

        // All trip service_ids should reference existing calendar entries
        tripServiceIds.Should().OnlyContain(id => serviceIds.Contains(id));
    }

    #endregion

    #region Unicode and Special Characters Tests

    [Fact]
    public async Task ParseAsync_WithGermanUmlauts_ShouldParseCorrectly()
    {
        // Arrange
        await CreateFile("stops.txt", """
            stop_id,stop_name,stop_lat,stop_lon
            S001,München Hauptbahnhof,48.140229,11.558338
            S002,Köln Hbf,50.943054,6.958731
            S003,Düsseldorf Hbf,51.220300,6.793140
            S004,Nürnberg Hbf,49.446042,11.082489
            """);

        await CreateFile("routes.txt", "route_id,route_short_name,route_type\nR001,RE,2");
        await CreateFile("trips.txt", "route_id,service_id,trip_id\nR001,WD,T001");
        await CreateFile("calendar.txt", "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\nWD,1,1,1,1,1,0,0,20240101,20241231");
        await CreateFile("stop_times.txt", "trip_id,arrival_time,departure_time,stop_id,stop_sequence\nT001,08:00:00,08:00:00,S001,1");

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        dataSet.Stops.Should().HaveCount(4);
        dataSet.Stops.Should().Contain(s => s.StopName == "München Hauptbahnhof");
        dataSet.Stops.Should().Contain(s => s.StopName == "Köln Hbf");
        dataSet.Stops.Should().Contain(s => s.StopName == "Düsseldorf Hbf");
        dataSet.Stops.Should().Contain(s => s.StopName == "Nürnberg Hbf");
    }

    [Fact]
    public async Task ParseAsync_WithQuotedCommas_ShouldParseCorrectly()
    {
        // Arrange
        await CreateFile("stops.txt", """
            stop_id,stop_name,stop_desc,stop_lat,stop_lon
            S001,"Hamburg, Hauptbahnhof","Main station, city center",53.552736,10.006909
            """);

        await CreateFile("routes.txt", "route_id,route_short_name,route_type\nR001,U1,1");
        await CreateFile("trips.txt", "route_id,service_id,trip_id\nR001,WD,T001");
        await CreateFile("calendar.txt", "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\nWD,1,1,1,1,1,0,0,20240101,20241231");
        await CreateFile("stop_times.txt", "trip_id,arrival_time,departure_time,stop_id,stop_sequence\nT001,08:00:00,08:00:00,S001,1");

        // Act
        var dataSet = await _parser.ParseAsync(_gtfsDirectory);

        // Assert
        var stop = dataSet.Stops.First();
        stop.StopName.Should().Be("Hamburg, Hauptbahnhof");
        stop.StopDesc.Should().Be("Main station, city center");
    }

    #endregion
}
