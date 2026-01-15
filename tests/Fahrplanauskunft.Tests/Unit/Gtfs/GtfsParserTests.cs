using Fahrplanauskunft.Infrastructure.Gtfs;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fahrplanauskunft.Tests.Unit.Gtfs;

/// <summary>
/// Unit tests for the <see cref="GtfsParser"/> class.
/// Tests verify individual parse methods with sample CSV data.
/// </summary>
public class GtfsParserTests : IDisposable
{
    private readonly ILogger<GtfsParser> _logger;
    private readonly GtfsParser _parser;
    private readonly string _testDirectory;

    public GtfsParserTests()
    {
        _logger = Substitute.For<ILogger<GtfsParser>>();
        _parser = new GtfsParser(_logger);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"GtfsParserTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new GtfsParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Act
        var parser = new GtfsParser(_logger);

        // Assert
        parser.Should().NotBeNull();
    }

    #endregion

    #region ParseStopsAsync Tests

    [Fact]
    public async Task ParseStopsAsync_WithValidStopsFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_code,stop_name,stop_desc,stop_lat,stop_lon,zone_id,location_type,parent_station
            S001,ABC1,Central Station,Main train station,53.5530,9.9930,A,1,
            S002,ABC2,East Station,East side station,53.5510,10.0020,A,0,S001
            S003,ABC3,West Station,West side station,53.5480,9.9800,B,0,
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(3);

        var centralStation = stops.First(s => s.StopId == "S001");
        centralStation.StopCode.Should().Be("ABC1");
        centralStation.StopName.Should().Be("Central Station");
        centralStation.StopDesc.Should().Be("Main train station");
        centralStation.StopLat.Should().BeApproximately(53.553, 0.0001);
        centralStation.StopLon.Should().BeApproximately(9.993, 0.0001);
        centralStation.ZoneId.Should().Be("A");
        centralStation.LocationType.Should().Be(1);

        var eastStation = stops.First(s => s.StopId == "S002");
        eastStation.ParentStation.Should().Be("S001");
        eastStation.LocationType.Should().Be(0);
    }

    [Fact]
    public async Task ParseStopsAsync_WithMinimalStopsFile_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Central Station,53.5530,9.9930
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(1);
        var stop = stops[0];
        stop.StopId.Should().Be("S001");
        stop.StopName.Should().Be("Central Station");
        stop.StopCode.Should().BeNull();
        stop.StopDesc.Should().BeNull();
        stop.ZoneId.Should().BeNull();
        stop.LocationType.Should().BeNull();
    }

    [Fact]
    public async Task ParseStopsAsync_WithUnicodeCharacters_ShouldParseCorrectly()
    {
        // Arrange - German station names with umlauts
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,München Hauptbahnhof,48.1401,11.5583
            S002,Köln Hauptbahnhof,50.9432,6.9587
            S003,Düsseldorf Hbf,51.2200,6.7931
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(3);
        stops[0].StopName.Should().Be("München Hauptbahnhof");
        stops[1].StopName.Should().Be("Köln Hauptbahnhof");
        stops[2].StopName.Should().Be("Düsseldorf Hbf");
    }

    [Fact]
    public async Task ParseStopsAsync_WithEmptyFile_ShouldReturnEmptyList()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseStopsAsync_ReportsProgress()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Stop 1,53.5530,9.9930
            S002,Stop 2,53.5510,10.0020
            """;
        await CreateTestFile("stops.txt", csvContent);

        var progressReports = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

        // Act
        await _parser.ParseStopsAsync(_testDirectory, progress);

        // Allow time for progress callbacks
        await Task.Delay(100);

        // Assert
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(p => p.CurrentFile == "stops.txt");
    }

    #endregion

    #region ParseRoutesAsync Tests

    [Fact]
    public async Task ParseRoutesAsync_WithValidRoutesFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            route_id,agency_id,route_short_name,route_long_name,route_desc,route_type,route_color,route_text_color
            R001,HVV,U1,U-Bahn Linie 1,Underground line from Norderstedt to Ohlstedt,1,0069B4,FFFFFF
            R002,HVV,S1,S-Bahn Linie 1,Suburban rail to Airport,2,009B3A,FFFFFF
            R003,HVV,3,Bus Linie 3,City bus line,3,E30613,FFFFFF
            """;
        await CreateTestFile("routes.txt", csvContent);

        // Act
        var routes = await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        routes.Should().HaveCount(3);

        var u1 = routes.First(r => r.RouteId == "R001");
        u1.AgencyId.Should().Be("HVV");
        u1.RouteShortName.Should().Be("U1");
        u1.RouteLongName.Should().Be("U-Bahn Linie 1");
        u1.RouteType.Should().Be(1); // Subway/Metro
        u1.RouteColor.Should().Be("0069B4");
        u1.RouteTextColor.Should().Be("FFFFFF");

        var s1 = routes.First(r => r.RouteId == "R002");
        s1.RouteType.Should().Be(2); // Rail

        var bus = routes.First(r => r.RouteId == "R003");
        bus.RouteType.Should().Be(3); // Bus
    }

    [Fact]
    public async Task ParseRoutesAsync_WithMinimalRoutesFile_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = """
            route_id,route_short_name,route_type
            R001,U1,1
            """;
        await CreateTestFile("routes.txt", csvContent);

        // Act
        var routes = await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        routes.Should().HaveCount(1);
        var route = routes[0];
        route.RouteId.Should().Be("R001");
        route.RouteShortName.Should().Be("U1");
        route.RouteType.Should().Be(1);
        route.AgencyId.Should().BeNull();
        route.RouteLongName.Should().BeNull();
        route.RouteColor.Should().BeNull();
    }

    [Fact]
    public async Task ParseRoutesAsync_WithAllStandardRouteTypes_ShouldParseCorrectly()
    {
        // Arrange - All standard GTFS route types
        var csvContent = """
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
        await CreateTestFile("routes.txt", csvContent);

        // Act
        var routes = await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        routes.Should().HaveCount(10);
        routes.Select(r => r.RouteType).Should().BeEquivalentTo([0, 1, 2, 3, 4, 5, 6, 7, 11, 12]);
    }

    #endregion

    #region ParseTripsAsync Tests

    [Fact]
    public async Task ParseTripsAsync_WithValidTripsFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            route_id,service_id,trip_id,trip_headsign,trip_short_name,direction_id,block_id,shape_id,wheelchair_accessible,bikes_allowed
            R001,WD,T001,Norderstedt Mitte,U1-001,0,B001,SH001,1,1
            R001,WD,T002,Ohlstedt,U1-002,1,B002,SH002,1,0
            R002,WE,T003,Airport,S1-001,0,B003,SH003,1,1
            """;
        await CreateTestFile("trips.txt", csvContent);

        // Act
        var trips = await _parser.ParseTripsAsync(_testDirectory);

        // Assert
        trips.Should().HaveCount(3);

        var trip1 = trips.First(t => t.TripId == "T001");
        trip1.RouteId.Should().Be("R001");
        trip1.ServiceId.Should().Be("WD");
        trip1.TripHeadsign.Should().Be("Norderstedt Mitte");
        trip1.TripShortName.Should().Be("U1-001");
        trip1.DirectionId.Should().Be(0);
        trip1.BlockId.Should().Be("B001");
        trip1.ShapeId.Should().Be("SH001");
        trip1.WheelchairAccessible.Should().Be(1);
        trip1.BikesAllowed.Should().Be(1);

        var trip2 = trips.First(t => t.TripId == "T002");
        trip2.DirectionId.Should().Be(1); // Opposite direction
        trip2.BikesAllowed.Should().Be(0);
    }

    [Fact]
    public async Task ParseTripsAsync_WithMinimalTripsFile_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = """
            route_id,service_id,trip_id
            R001,WD,T001
            """;
        await CreateTestFile("trips.txt", csvContent);

        // Act
        var trips = await _parser.ParseTripsAsync(_testDirectory);

        // Assert
        trips.Should().HaveCount(1);
        var trip = trips[0];
        trip.RouteId.Should().Be("R001");
        trip.ServiceId.Should().Be("WD");
        trip.TripId.Should().Be("T001");
        trip.TripHeadsign.Should().BeNull();
        trip.DirectionId.Should().BeNull();
        trip.WheelchairAccessible.Should().BeNull();
    }

    #endregion

    #region ParseCalendarsAsync Tests

    [Fact]
    public async Task ParseCalendarsAsync_WithValidCalendarFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            WE,0,0,0,0,0,1,1,20240101,20241231
            DAILY,1,1,1,1,1,1,1,20240101,20241231
            """;
        await CreateTestFile("calendar.txt", csvContent);

        // Act
        var calendars = await _parser.ParseCalendarsAsync(_testDirectory);

        // Assert
        calendars.Should().HaveCount(3);

        var weekday = calendars.First(c => c.ServiceId == "WD");
        weekday.Monday.Should().Be(1);
        weekday.Tuesday.Should().Be(1);
        weekday.Wednesday.Should().Be(1);
        weekday.Thursday.Should().Be(1);
        weekday.Friday.Should().Be(1);
        weekday.Saturday.Should().Be(0);
        weekday.Sunday.Should().Be(0);
        weekday.StartDate.Should().Be("20240101");
        weekday.EndDate.Should().Be("20241231");

        var weekend = calendars.First(c => c.ServiceId == "WE");
        weekend.Monday.Should().Be(0);
        weekend.Saturday.Should().Be(1);
        weekend.Sunday.Should().Be(1);

        var daily = calendars.First(c => c.ServiceId == "DAILY");
        daily.Monday.Should().Be(1);
        daily.Sunday.Should().Be(1);
    }

    [Fact]
    public async Task ParseCalendarsAsync_DateFormat_ShouldBeStoredAsString()
    {
        // Arrange
        var csvContent = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            SVC,1,1,1,1,1,1,1,20231215,20240315
            """;
        await CreateTestFile("calendar.txt", csvContent);

        // Act
        var calendars = await _parser.ParseCalendarsAsync(_testDirectory);

        // Assert
        calendars.Should().HaveCount(1);
        var calendar = calendars[0];
        calendar.StartDate.Should().Be("20231215");
        calendar.EndDate.Should().Be("20240315");
    }

    #endregion

    #region ParseStopTimesAsync Tests

    [Fact]
    public async Task ParseStopTimesAsync_WithValidStopTimesFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,stop_headsign,pickup_type,drop_off_type
            T001,08:00:00,08:00:00,S001,1,,0,0
            T001,08:05:00,08:06:00,S002,2,,0,0
            T001,08:12:00,08:12:00,S003,3,Norderstedt,0,0
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(3);

        var firstStop = stopTimes.First(st => st.StopSequence == 1);
        firstStop.TripId.Should().Be("T001");
        firstStop.ArrivalTime.Should().Be("08:00:00");
        firstStop.DepartureTime.Should().Be("08:00:00");
        firstStop.StopId.Should().Be("S001");
        firstStop.PickupType.Should().Be(0);
        firstStop.DropOffType.Should().Be(0);

        var lastStop = stopTimes.First(st => st.StopSequence == 3);
        lastStop.StopHeadsign.Should().Be("Norderstedt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = """
            trip_id,stop_id,stop_sequence
            T001,S001,1
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(1);
        var stopTime = stopTimes[0];
        stopTime.TripId.Should().Be("T001");
        stopTime.StopId.Should().Be("S001");
        stopTime.StopSequence.Should().Be(1);
        stopTime.ArrivalTime.Should().BeNull();
        stopTime.DepartureTime.Should().BeNull();
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithNonConsecutiveSequence_ShouldParseCorrectly()
    {
        // Arrange - GTFS allows non-consecutive stop sequences
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,1
            T001,08:10:00,08:11:00,S002,5
            T001,08:25:00,08:26:00,S003,10
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(3);
        stopTimes[0].StopSequence.Should().Be(1);
        stopTimes[1].StopSequence.Should().Be(5);
        stopTimes[2].StopSequence.Should().Be(10);
    }

    [Fact]
    public async Task ParseStopTimesAsync_ReportsProgressAtIntervals()
    {
        // Arrange
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,1
            T001,08:05:00,08:06:00,S002,2
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        var progressReports = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

        // Act
        await _parser.ParseStopTimesAsync(_testDirectory, progress);

        // Allow time for progress callbacks
        await Task.Delay(100);

        // Assert
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(p => p.CurrentFile == "stop_times.txt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithExtendedTimeFormat_ShouldPreserveTimeAsString()
    {
        // Arrange - GTFS allows times greater than 24:00:00 for trips spanning past midnight
        // For example, 25:30:00 represents 1:30 AM the next day
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,23:45:00,23:45:00,S001,1
            T001,24:00:00,24:01:00,S002,2
            T001,24:30:00,24:31:00,S003,3
            T001,25:30:00,25:32:00,S004,4
            T001,26:15:00,26:15:00,S005,5
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(5);

        // Regular time before midnight
        var stop1 = stopTimes.First(st => st.StopSequence == 1);
        stop1.ArrivalTime.Should().Be("23:45:00");
        stop1.DepartureTime.Should().Be("23:45:00");

        // Exactly midnight (24:00:00)
        var stop2 = stopTimes.First(st => st.StopSequence == 2);
        stop2.ArrivalTime.Should().Be("24:00:00");
        stop2.DepartureTime.Should().Be("24:01:00");

        // 30 minutes past midnight (24:30:00)
        var stop3 = stopTimes.First(st => st.StopSequence == 3);
        stop3.ArrivalTime.Should().Be("24:30:00");
        stop3.DepartureTime.Should().Be("24:31:00");

        // 1:30 AM the next day (25:30:00)
        var stop4 = stopTimes.First(st => st.StopSequence == 4);
        stop4.ArrivalTime.Should().Be("25:30:00");
        stop4.DepartureTime.Should().Be("25:32:00");

        // 2:15 AM the next day (26:15:00)
        var stop5 = stopTimes.First(st => st.StopSequence == 5);
        stop5.ArrivalTime.Should().Be("26:15:00");
        stop5.DepartureTime.Should().Be("26:15:00");
    }

    [Theory]
    [InlineData("25:30:00", "1:30 AM next day")]
    [InlineData("26:00:00", "2:00 AM next day")]
    [InlineData("27:45:30", "3:45:30 AM next day")]
    [InlineData("30:00:00", "6:00 AM next day")]
    [InlineData("47:59:59", "23:59:59 next day (maximum reasonable)")]
    public async Task ParseStopTimesAsync_WithVariousExtendedTimes_ShouldPreserveExactly(string extendedTime, string description)
    {
        // Arrange - Test various extended time formats that GTFS supports
        var csvContent = $"""
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,{extendedTime},{extendedTime},S001,1
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(1, because: $"extended time {extendedTime} ({description}) should be parsed");
        stopTimes[0].ArrivalTime.Should().Be(extendedTime, because: $"time should be preserved as-is for {description}");
        stopTimes[0].DepartureTime.Should().Be(extendedTime, because: $"time should be preserved as-is for {description}");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithMixedNormalAndExtendedTimes_ShouldParseAll()
    {
        // Arrange - A realistic overnight trip scenario
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,stop_headsign
            NIGHT001,22:00:00,22:00:00,S001,1,Night Express
            NIGHT001,22:30:00,22:31:00,S002,2,
            NIGHT001,23:15:00,23:16:00,S003,3,
            NIGHT001,23:59:00,24:00:00,S004,4,
            NIGHT001,24:30:00,24:31:00,S005,5,
            NIGHT001,25:00:00,25:00:00,S006,6,End Station
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(6);

        // All trip IDs should be consistent
        stopTimes.Should().OnlyContain(st => st.TripId == "NIGHT001");

        // Verify times are preserved correctly
        stopTimes.Select(st => st.ArrivalTime).Should().BeEquivalentTo(
            ["22:00:00", "22:30:00", "23:15:00", "23:59:00", "24:30:00", "25:00:00"],
            options => options.WithStrictOrdering());

        // Verify stop headsigns are parsed correctly alongside extended times
        stopTimes.First(st => st.StopSequence == 1).StopHeadsign.Should().Be("Night Express");
        stopTimes.First(st => st.StopSequence == 6).StopHeadsign.Should().Be("End Station");
    }

    #endregion

    #region ParseTransfersAsync Tests

    [Fact]
    public async Task ParseTransfersAsync_WithValidTransfersFile_ShouldParseAllRecords()
    {
        // Arrange
        var csvContent = """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time,from_route_id,to_route_id
            S001,S002,2,180,R001,R002
            S002,S003,1,,R002,R003
            S003,S001,0,,,
            """;
        await CreateTestFile("transfers.txt", csvContent);

        // Act
        var transfers = await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        transfers.Should().HaveCount(3);

        var transfer1 = transfers.First(t => t.FromStopId == "S001");
        transfer1.ToStopId.Should().Be("S002");
        transfer1.TransferType.Should().Be(2); // Minimum transfer time required
        transfer1.MinTransferTime.Should().Be(180);
        transfer1.FromRouteId.Should().Be("R001");
        transfer1.ToRouteId.Should().Be("R002");

        var transfer2 = transfers.First(t => t.FromStopId == "S002");
        transfer2.TransferType.Should().Be(1); // Timed transfer
        transfer2.MinTransferTime.Should().BeNull();

        var transfer3 = transfers.First(t => t.FromStopId == "S003");
        transfer3.TransferType.Should().Be(0); // Recommended transfer
    }

    [Fact]
    public async Task ParseTransfersAsync_WithMinimalFields_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = """
            from_stop_id,to_stop_id,transfer_type
            S001,S002,0
            """;
        await CreateTestFile("transfers.txt", csvContent);

        // Act
        var transfers = await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        transfers.Should().HaveCount(1);
        var transfer = transfers[0];
        transfer.FromStopId.Should().Be("S001");
        transfer.ToStopId.Should().Be("S002");
        transfer.TransferType.Should().Be(0);
        transfer.MinTransferTime.Should().BeNull();
        transfer.FromRouteId.Should().BeNull();
        transfer.ToRouteId.Should().BeNull();
    }

    [Fact]
    public async Task ParseTransfersAsync_WithMissingFile_ShouldReturnEmptyList()
    {
        // Arrange - No transfers.txt file created

        // Act
        var transfers = await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        transfers.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, "Recommended transfer point")]
    [InlineData(1, "Timed transfer point")]
    [InlineData(2, "Minimum transfer time required")]
    [InlineData(3, "Transfers not possible")]
    [InlineData(4, "In-seat transfer")]
    [InlineData(5, "In-seat transfer not allowed")]
    public async Task ParseTransfersAsync_AllTransferTypes_ShouldParseCorrectly(int transferType, string description)
    {
        // Arrange
        var csvContent = $"""
            from_stop_id,to_stop_id,transfer_type
            S001,S002,{transferType}
            """;
        await CreateTestFile("transfers.txt", csvContent);

        // Act
        var transfers = await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        transfers.Should().HaveCount(1);
        transfers[0].TransferType.Should().Be(transferType, because: description);
    }

    #endregion

    #region ParseAsync (Orchestrator) Tests

    [Fact]
    public async Task ParseAsync_WithAllRequiredFiles_ShouldReturnCompleteDataSet()
    {
        // Arrange
        await CreateAllRequiredTestFiles();

        // Act
        var dataSet = await _parser.ParseAsync(_testDirectory);

        // Assert
        dataSet.Should().NotBeNull();
        dataSet.Stops.Should().NotBeEmpty();
        dataSet.Routes.Should().NotBeEmpty();
        dataSet.Trips.Should().NotBeEmpty();
        dataSet.Calendars.Should().NotBeEmpty();
        dataSet.StopTimes.Should().NotBeEmpty();
        dataSet.HasRequiredData.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_WithAllRequiredFilesAndTransfers_ShouldIncludeTransfers()
    {
        // Arrange
        await CreateAllRequiredTestFiles();
        var transfersCsv = """
            from_stop_id,to_stop_id,transfer_type
            S001,S002,0
            """;
        await CreateTestFile("transfers.txt", transfersCsv);

        // Act
        var dataSet = await _parser.ParseAsync(_testDirectory);

        // Assert
        dataSet.Transfers.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseAsync_WithoutTransfersFile_ShouldReturnEmptyTransfers()
    {
        // Arrange
        await CreateAllRequiredTestFiles();
        // No transfers.txt file

        // Act
        var dataSet = await _parser.ParseAsync(_testDirectory);

        // Assert
        dataSet.Transfers.Should().BeEmpty();
        dataSet.HasRequiredData.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_WithNonExistentDirectory_ShouldThrowDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var act = async () => await _parser.ParseAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage($"*{nonExistentPath}*");
    }

    [Fact]
    public async Task ParseAsync_WithMissingRequiredFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - Create all files except stops.txt
        await CreateTestFile("routes.txt", "route_id,route_short_name,route_type\nR001,U1,1");
        await CreateTestFile("trips.txt", "route_id,service_id,trip_id\nR001,WD,T001");
        await CreateTestFile("calendar.txt", "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\nWD,1,1,1,1,1,0,0,20240101,20241231");
        await CreateTestFile("stop_times.txt", "trip_id,stop_id,stop_sequence\nT001,S001,1");
        // Missing stops.txt

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory);

        // Assert
        await act.Should().ThrowAsync<GtfsParsingException>()
            .Where(ex => ex.MissingFiles != null && ex.MissingFiles.Contains("stops.txt"));
    }

    [Fact]
    public async Task ParseAsync_WithNullDirectoryPath_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _parser.ParseAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("directoryPath");
    }

    [Fact]
    public async Task ParseAsync_ReportsProgress()
    {
        // Arrange
        await CreateAllRequiredTestFiles();
        var progressReports = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

        // Act
        await _parser.ParseAsync(_testDirectory, progress);

        // Allow time for progress callbacks
        await Task.Delay(200);

        // Assert
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(p => p.CurrentFile == "stops.txt");
        progressReports.Should().Contain(p => p.CurrentFile == "routes.txt");
        progressReports.Should().Contain(p => p.CurrentFile == "trips.txt");
        progressReports.Should().Contain(p => p.CurrentFile == "calendar.txt");
        progressReports.Should().Contain(p => p.CurrentFile == "stop_times.txt");
    }

    [Fact]
    public async Task ParseAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        await CreateAllRequiredTestFiles();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region BOM Handling Tests

    [Fact]
    public async Task ParseStopsAsync_WithBomEncoding_ShouldParseCorrectly()
    {
        // Arrange - Write file with BOM (UTF-8 with BOM)
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Central Station,53.5530,9.9930
            """;
        var filePath = Path.Combine(_testDirectory, "stops.txt");
        await File.WriteAllTextAsync(filePath, csvContent, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopId.Should().Be("S001"); // Should not contain BOM
        stops[0].StopId.Should().NotStartWith("\uFEFF"); // Should not start with BOM
    }

    #endregion

    #region Multiple Records Performance Tests

    [Fact]
    public async Task ParseStopsAsync_WithManyRecords_ShouldParseAllCorrectly()
    {
        // Arrange - Create 100 stops
        var csvLines = new List<string> { "stop_id,stop_name,stop_lat,stop_lon" };
        for (int i = 1; i <= 100; i++)
        {
            csvLines.Add($"S{i:D3},Stop {i},{53.5 + (i * 0.001)},{9.9 + (i * 0.001)}");
        }
        var csvContent = string.Join("\n", csvLines);
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(100);
        stops.First().StopId.Should().Be("S001");
        stops.Last().StopId.Should().Be("S100");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithManyRecords_ShouldParseAllCorrectly()
    {
        // Arrange - Create 500 stop times (simulating a large trip sequence)
        var csvLines = new List<string> { "trip_id,arrival_time,departure_time,stop_id,stop_sequence" };
        for (int i = 1; i <= 500; i++)
        {
            var hour = 8 + (i / 60);
            var minute = i % 60;
            csvLines.Add($"T001,{hour:D2}:{minute:D2}:00,{hour:D2}:{minute:D2}:30,S{i:D3},{i}");
        }
        var csvContent = string.Join("\n", csvLines);
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var stopTimes = await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        stopTimes.Should().HaveCount(500);
        stopTimes.First().StopSequence.Should().Be(1);
        stopTimes.Last().StopSequence.Should().Be(500);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ParseAsync_WithEmptyDirectoryPath_ShouldThrowDirectoryNotFoundException()
    {
        // Act
        var act = async () => await _parser.ParseAsync(string.Empty);

        // Assert
        // Empty string is treated as current directory which doesn't have GTFS files
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task ParseAsync_DirectoryNotFoundException_ShouldContainPathInMessage()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");

        // Act
        var act = async () => await _parser.ParseAsync(nonExistentPath);

        // Assert
        var exception = await act.Should().ThrowAsync<DirectoryNotFoundException>();
        exception.WithMessage($"*GTFS directory not found*{nonExistentPath}*");
    }

    [Fact]
    public async Task ParseAsync_WithMultipleMissingRequiredFiles_ShouldListAllMissingFiles()
    {
        // Arrange - Empty directory with no GTFS files

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.MissingFiles.Should().Contain("stops.txt");
        exception.Which.MissingFiles.Should().Contain("routes.txt");
        exception.Which.MissingFiles.Should().Contain("trips.txt");
        exception.Which.MissingFiles.Should().Contain("stop_times.txt");
        exception.Which.MissingFiles.Should().Contain("calendar.txt");
        exception.Which.IsMissingFilesError.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_MissingOnlyStopsFile_ShouldThrowGtfsParsingExceptionForStops()
    {
        // Arrange - Create all files except stops.txt
        await CreateTestFile("routes.txt", "route_id,route_short_name,route_type\nR001,U1,1");
        await CreateTestFile("trips.txt", "route_id,service_id,trip_id\nR001,WD,T001");
        await CreateTestFile("calendar.txt", "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\nWD,1,1,1,1,1,0,0,20240101,20241231");
        await CreateTestFile("stop_times.txt", "trip_id,stop_id,stop_sequence\nT001,S001,1");

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.MissingFiles.Should().ContainSingle().Which.Should().Be("stops.txt");
    }

    [Fact]
    public async Task ParseAsync_MissingOnlyCalendarFile_ShouldThrowGtfsParsingExceptionForCalendar()
    {
        // Arrange - Create all files except calendar.txt
        await CreateTestFile("stops.txt", "stop_id,stop_name,stop_lat,stop_lon\nS001,Station,53.55,9.99");
        await CreateTestFile("routes.txt", "route_id,route_short_name,route_type\nR001,U1,1");
        await CreateTestFile("trips.txt", "route_id,service_id,trip_id\nR001,WD,T001");
        await CreateTestFile("stop_times.txt", "trip_id,stop_id,stop_sequence\nT001,S001,1");

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.MissingFiles.Should().ContainSingle().Which.Should().Be("calendar.txt");
    }

    [Fact]
    public async Task ParseStopsAsync_WithInvalidLatitude_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid double value for latitude
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Station,invalid_lat,9.99
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stops.txt");
        exception.Which.HasFileContext.Should().BeTrue();
    }

    [Fact]
    public async Task ParseRoutesAsync_WithInvalidRouteType_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for route_type
        var csvContent = """
            route_id,route_short_name,route_type
            R001,U1,not_a_number
            """;
        await CreateTestFile("routes.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("routes.txt");
    }

    [Fact]
    public async Task ParseTripsAsync_WithInvalidDirectionId_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for direction_id
        var csvContent = """
            route_id,service_id,trip_id,direction_id
            R001,WD,T001,invalid
            """;
        await CreateTestFile("trips.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseTripsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("trips.txt");
    }

    [Fact]
    public async Task ParseCalendarsAsync_WithInvalidDayFlag_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for monday flag
        var csvContent = """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,invalid,1,1,1,1,0,0,20240101,20241231
            """;
        await CreateTestFile("calendar.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseCalendarsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("calendar.txt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithInvalidStopSequence_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for stop_sequence
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,not_a_number
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stop_times.txt");
    }

    [Fact]
    public async Task ParseTransfersAsync_WithInvalidTransferType_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for transfer_type
        var csvContent = """
            from_stop_id,to_stop_id,transfer_type
            S001,S002,invalid
            """;
        await CreateTestFile("transfers.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("transfers.txt");
    }

    [Fact]
    public async Task ParseTransfersAsync_WithInvalidMinTransferTime_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for min_transfer_time
        var csvContent = """
            from_stop_id,to_stop_id,transfer_type,min_transfer_time
            S001,S002,2,invalid_time
            """;
        await CreateTestFile("transfers.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseTransfersAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("transfers.txt");
    }

    [Fact]
    public async Task ParseStopsAsync_WithMissingStopsFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - No stops.txt file exists

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stops.txt");
        exception.Which.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ParseRoutesAsync_WithMissingRoutesFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - No routes.txt file exists

        // Act
        var act = async () => await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("routes.txt");
    }

    [Fact]
    public async Task ParseTripsAsync_WithMissingTripsFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - No trips.txt file exists

        // Act
        var act = async () => await _parser.ParseTripsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("trips.txt");
    }

    [Fact]
    public async Task ParseCalendarsAsync_WithMissingCalendarFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - No calendar.txt file exists

        // Act
        var act = async () => await _parser.ParseCalendarsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("calendar.txt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithMissingStopTimesFile_ShouldThrowGtfsParsingException()
    {
        // Arrange - No stop_times.txt file exists

        // Act
        var act = async () => await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stop_times.txt");
    }

    [Fact]
    public async Task GtfsParsingException_WithFileContext_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Station,invalid,9.99
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.Message.Should().Contain("stops.txt");
        exception.Which.HasFileContext.Should().BeTrue();
        exception.Which.IsMissingFilesError.Should().BeFalse();
    }

    [Fact]
    public async Task GtfsParsingException_ForMissingFiles_ShouldHaveCorrectProperties()
    {
        // Arrange - Empty directory

        // Act
        var act = async () => await _parser.ParseAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.IsMissingFilesError.Should().BeTrue();
        exception.Which.MissingFiles.Should().HaveCount(5);
        exception.Which.HasFileContext.Should().BeFalse();
        exception.Which.FileName.Should().BeNull();
        exception.Which.LineNumber.Should().BeNull();
    }

    [Fact]
    public async Task ParseStopsAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Station,53.55,9.99
            """;
        await CreateTestFile("stops.txt", csvContent);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ParseRoutesAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var csvContent = """
            route_id,route_short_name,route_type
            R001,U1,1
            """;
        await CreateTestFile("routes.txt", csvContent);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _parser.ParseRoutesAsync(_testDirectory, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ParseStopsAsync_WithInvalidLongitude_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid double value for longitude
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Station,53.55,invalid_lon
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stops.txt");
    }

    [Fact]
    public async Task ParseStopsAsync_WithInvalidLocationType_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for location_type
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon,location_type
            S001,Station,53.55,9.99,not_integer
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stops.txt");
    }

    [Fact]
    public async Task ParseTripsAsync_WithInvalidWheelchairAccessible_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for wheelchair_accessible
        var csvContent = """
            route_id,service_id,trip_id,wheelchair_accessible
            R001,WD,T001,invalid
            """;
        await CreateTestFile("trips.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseTripsAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("trips.txt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithInvalidPickupType_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for pickup_type
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,pickup_type
            T001,08:00:00,08:00:00,S001,1,invalid
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stop_times.txt");
    }

    [Fact]
    public async Task ParseStopTimesAsync_WithInvalidDropOffType_ShouldThrowGtfsParsingException()
    {
        // Arrange - Invalid integer value for drop_off_type
        var csvContent = """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence,drop_off_type
            T001,08:00:00,08:00:00,S001,1,invalid
            """;
        await CreateTestFile("stop_times.txt", csvContent);

        // Act
        var act = async () => await _parser.ParseStopTimesAsync(_testDirectory);

        // Assert
        var exception = await act.Should().ThrowAsync<GtfsParsingException>();
        exception.Which.FileName.Should().Be("stop_times.txt");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ParseStopsAsync_WithQuotedValues_ShouldParseCorrectly()
    {
        // Arrange - CSV with quoted values containing commas
        var csvContent = """
            stop_id,stop_name,stop_desc,stop_lat,stop_lon
            S001,"Central Station, Main Platform","Large station, multiple entrances",53.5530,9.9930
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopName.Should().Be("Central Station, Main Platform");
        stops[0].StopDesc.Should().Be("Large station, multiple entrances");
    }

    [Fact]
    public async Task ParseRoutesAsync_WithEmptyOptionalFields_ShouldParseCorrectly()
    {
        // Arrange
        var csvContent = """
            route_id,agency_id,route_short_name,route_long_name,route_type,route_color,route_text_color
            R001,,U1,,1,,
            """;
        await CreateTestFile("routes.txt", csvContent);

        // Act
        var routes = await _parser.ParseRoutesAsync(_testDirectory);

        // Assert
        routes.Should().HaveCount(1);
        var route = routes[0];
        route.RouteId.Should().Be("R001");
        route.AgencyId.Should().BeEmpty();
        route.RouteShortName.Should().Be("U1");
        route.RouteLongName.Should().BeEmpty();
        route.RouteType.Should().Be(1);
        route.RouteColor.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseStopsAsync_WithNegativeCoordinates_ShouldParseCorrectly()
    {
        // Arrange - Southern/Western hemisphere coordinates
        var csvContent = """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Buenos Aires,-34.6037,-58.3816
            """;
        await CreateTestFile("stops.txt", csvContent);

        // Act
        var stops = await _parser.ParseStopsAsync(_testDirectory);

        // Assert
        stops.Should().HaveCount(1);
        stops[0].StopLat.Should().BeApproximately(-34.6037, 0.0001);
        stops[0].StopLon.Should().BeApproximately(-58.3816, 0.0001);
    }

    #endregion

    #region Helper Methods

    private async Task CreateTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task CreateAllRequiredTestFiles()
    {
        // stops.txt
        await CreateTestFile("stops.txt", """
            stop_id,stop_name,stop_lat,stop_lon
            S001,Central Station,53.5530,9.9930
            S002,East Station,53.5510,10.0020
            """);

        // routes.txt
        await CreateTestFile("routes.txt", """
            route_id,route_short_name,route_type
            R001,U1,1
            """);

        // trips.txt
        await CreateTestFile("trips.txt", """
            route_id,service_id,trip_id
            R001,WD,T001
            """);

        // calendar.txt
        await CreateTestFile("calendar.txt", """
            service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date
            WD,1,1,1,1,1,0,0,20240101,20241231
            """);

        // stop_times.txt
        await CreateTestFile("stop_times.txt", """
            trip_id,arrival_time,departure_time,stop_id,stop_sequence
            T001,08:00:00,08:00:00,S001,1
            T001,08:10:00,08:10:00,S002,2
            """);
    }

    #endregion
}
