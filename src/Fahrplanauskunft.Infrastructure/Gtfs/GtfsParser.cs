using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Parses GTFS (General Transit Feed Specification) CSV files from a directory.
/// </summary>
/// <remarks>
/// <para>
/// This parser reads standard GTFS CSV files and returns a populated <see cref="GtfsDataSet"/>.
/// It handles BOM (Byte Order Mark) encoding, extended time formats (e.g., 25:30:00 for next-day times),
/// and reports progress for large files.
/// </para>
/// <para>
/// Required GTFS files: stops.txt, routes.txt, trips.txt, stop_times.txt, calendar.txt.
/// Optional GTFS files: transfers.txt.
/// </para>
/// <para>
/// See: https://gtfs.org/schedule/reference/
/// </para>
/// </remarks>
public sealed partial class GtfsParser : IGtfsParser
{
    private readonly ILogger<GtfsParser> _logger;

    #region LoggerMessage Definitions

    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting to parse {FileName}")]
    private partial void LogStartingParse(string fileName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Parsed {RecordsProcessed} records from {FileName}")]
    private partial void LogParsingProgress(int recordsProcessed, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed parsing {FileName}: {RecordCount} records")]
    private partial void LogParsingCompleted(string fileName, int recordCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Parsing of {FileName} was canceled after processing {RecordsProcessed} records")]
    private partial void LogParsingCanceled(string fileName, int recordsProcessed);

    [LoggerMessage(Level = LogLevel.Error, Message = "GTFS file not found: {FilePath}")]
    private partial void LogFileNotFound(Exception ex, string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Bad data encountered in {FileName} at row {Row}")]
    private partial void LogBadData(Exception ex, string fileName, int? row);

    [LoggerMessage(Level = LogLevel.Error, Message = "Type conversion error in {FileName} at row {Row}")]
    private partial void LogTypeConversionError(Exception ex, string fileName, int? row);

    [LoggerMessage(Level = LogLevel.Error, Message = "Reader error in {FileName} at row {Row}")]
    private partial void LogReaderError(Exception ex, string fileName, int? row);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unexpected error parsing {FileName}")]
    private partial void LogUnexpectedError(Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Optional file {FileName} not found, returning empty collection")]
    private partial void LogOptionalFileNotFound(string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting GTFS parsing from directory: {DirectoryPath}")]
    private partial void LogStartingGtfsParsing(string directoryPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Required GTFS files missing: {MissingFiles}")]
    private partial void LogMissingRequiredFiles(string missingFiles);

    [LoggerMessage(Level = LogLevel.Information, Message = "GTFS parsing completed. Stops: {StopsCount}, Routes: {RoutesCount}, Trips: {TripsCount}, StopTimes: {StopTimesCount}, Calendars: {CalendarsCount}, Transfers: {TransfersCount}. Total: {TotalCount} entities")]
    private partial void LogParsingCompletedSummary(int stopsCount, int routesCount, int tripsCount, int stopTimesCount, int calendarsCount, int transfersCount, int totalCount);

    #endregion

    /// <summary>
    /// The interval at which progress is reported during parsing of large files.
    /// Progress is reported every 10,000 records to minimize callback overhead.
    /// </summary>
    private const int ProgressReportInterval = 10_000;

    /// <summary>
    /// Default buffer size for file streams (64 KB).
    /// Larger buffers improve I/O performance for large files like stop_times.txt.
    /// </summary>
    private const int DefaultBufferSize = 65536;

    /// <summary>
    /// Estimated record counts for pre-sizing collections.
    /// These are typical sizes for medium-to-large transit agencies.
    /// Pre-allocation prevents multiple array resizes during parsing.
    /// </summary>
    private static class EstimatedRecordCounts
    {
        public const int Stops = 5000;
        public const int Routes = 200;
        public const int Trips = 10000;
        public const int StopTimes = 500000;
        public const int Calendars = 50;
        public const int Transfers = 1000;
    }

    /// <summary>
    /// The list of required GTFS file names that must exist in the directory.
    /// </summary>
    private static readonly string[] RequiredFiles =
    [
        "stops.txt",
        "routes.txt",
        "trips.txt",
        "stop_times.txt",
        "calendar.txt"
    ];

    /// <summary>
    /// Cached CsvHelper configuration instance.
    /// Created once and reused for all file parsing operations.
    /// </summary>
    private static readonly CsvConfiguration CachedCsvConfiguration = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null,
        HeaderValidated = null,
        DetectDelimiter = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GtfsParser"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging parsing operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public GtfsParser(ILogger<GtfsParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<GtfsDataSet> ParseAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        LogStartingGtfsParsing(directoryPath);

        // Validate directory exists
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"GTFS directory not found: {directoryPath}");
        }

        // Check for required files (avoid LINQ allocation for common success case)
        List<string>? missingFiles = null;
        foreach (var file in RequiredFiles)
        {
            if (!File.Exists(Path.Combine(directoryPath, file)))
            {
                missingFiles ??= new List<string>(RequiredFiles.Length);
                missingFiles.Add(file);
            }
        }

        if (missingFiles is { Count: > 0 })
        {
            LogMissingRequiredFiles(string.Join(", ", missingFiles));
            throw new GtfsParsingException(missingFiles);
        }

        // Check cancellation before starting
        cancellationToken.ThrowIfCancellationRequested();

        // Parse smaller files in parallel for better performance
        // These are typically small (stops ~5K, routes ~200, trips ~10K, calendars ~50)
        var stopsTask = ParseStopsAsync(directoryPath, progress, cancellationToken);
        var routesTask = ParseRoutesAsync(directoryPath, progress, cancellationToken);
        var tripsTask = ParseTripsAsync(directoryPath, progress, cancellationToken);
        var calendarsTask = ParseCalendarsAsync(directoryPath, progress, cancellationToken);

        // Wait for all small file parsing to complete
        await Task.WhenAll(stopsTask, routesTask, tripsTask, calendarsTask).ConfigureAwait(false);

        var stops = stopsTask.Result;
        var routes = routesTask.Result;
        var trips = tripsTask.Result;
        var calendars = calendarsTask.Result;

        cancellationToken.ThrowIfCancellationRequested();

        // Parse large files sequentially to avoid memory pressure
        // stop_times.txt is typically the largest file (100K-1M+ records)
        var stopTimes = await ParseStopTimesAsync(directoryPath, progress, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        // transfers.txt is optional and typically small
        var transfers = await ParseTransfersAsync(directoryPath, progress, cancellationToken).ConfigureAwait(false);

        // Build the result
        var dataSet = new GtfsDataSet
        {
            Stops = stops,
            Routes = routes,
            Trips = trips,
            Calendars = calendars,
            StopTimes = stopTimes,
            Transfers = transfers
        };

        // Log completion summary
        LogParsingCompletedSummary(
            stops.Count,
            routes.Count,
            trips.Count,
            stopTimes.Count,
            calendars.Count,
            transfers.Count,
            dataSet.TotalEntityCount);

        return dataSet;
    }

    #region Generic CSV Parsing Infrastructure

    /// <summary>
    /// Gets the estimated record count for pre-sizing the list.
    /// Returns a sensible default for unknown file types.
    /// </summary>
    private static int GetEstimatedRecordCount(string fileName) => fileName switch
    {
        "stops.txt" => EstimatedRecordCounts.Stops,
        "routes.txt" => EstimatedRecordCounts.Routes,
        "trips.txt" => EstimatedRecordCounts.Trips,
        "stop_times.txt" => EstimatedRecordCounts.StopTimes,
        "calendar.txt" => EstimatedRecordCounts.Calendars,
        "transfers.txt" => EstimatedRecordCounts.Transfers,
        _ => 1000
    };

    /// <summary>
    /// Generic method to parse a GTFS CSV file into a list of entities.
    /// Reduces code duplication across all parsing methods by centralizing common logic.
    /// </summary>
    /// <typeparam name="T">The GTFS entity type to parse (e.g., GtfsStop, GtfsRoute).</typeparam>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="fileName">The name of the file to parse (e.g., "stops.txt").</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    private async Task<List<T>> ParseCsvFileAsync<T>(
        string directoryPath,
        string fileName,
        IProgress<ImportProgress>? progress,
        CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(directoryPath, fileName);

        LogStartingParse(fileName);

        // Pre-allocate list capacity based on estimated record count to avoid repeated array resizing
        var estimatedCapacity = GetEstimatedRecordCount(fileName);
        var entities = new List<T>(estimatedCapacity);
        var recordsProcessed = 0;

        try
        {
            progress?.Report(new ImportProgress(fileName, 0, null, "Starting"));

            // Use larger buffer for better I/O performance with large files
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: DefaultBufferSize, useAsync: true);
            using var streamReader = new StreamReader(fileStream, detectEncodingFromByteOrderMarks: true);
            using var csvReader = new CsvReader(streamReader, CachedCsvConfiguration);

            await foreach (var entity in csvReader.GetRecordsAsync<T>(cancellationToken))
            {
                entities.Add(entity);
                recordsProcessed++;

                if (recordsProcessed % ProgressReportInterval == 0)
                {
                    progress?.Report(new ImportProgress(fileName, recordsProcessed, null, "Parsing"));
                    LogParsingProgress(recordsProcessed, fileName);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            progress?.Report(new ImportProgress(fileName, recordsProcessed, recordsProcessed, "Completed"));
            LogParsingCompleted(fileName, recordsProcessed);

            return entities;
        }
        catch (OperationCanceledException)
        {
            LogParsingCanceled(fileName, recordsProcessed);
            throw;
        }
        catch (FileNotFoundException ex)
        {
            LogFileNotFound(ex, filePath);
            throw new GtfsParsingException($"Required GTFS file not found: {fileName}", fileName, null, ex);
        }
        catch (CsvHelper.BadDataException ex)
        {
            LogBadData(ex, fileName, ex.Context?.Parser?.Row);
            throw new GtfsParsingException($"Bad data encountered: {ex.Message}", fileName, ex.Context?.Parser?.Row, ex);
        }
        catch (CsvHelper.TypeConversion.TypeConverterException ex)
        {
            LogTypeConversionError(ex, fileName, ex.Context?.Parser?.Row);
            throw new GtfsParsingException($"Type conversion error: {ex.Message}", fileName, ex.Context?.Parser?.Row, ex);
        }
        catch (CsvHelper.ReaderException ex)
        {
            LogReaderError(ex, fileName, ex.Context?.Parser?.Row);
            throw new GtfsParsingException($"CSV reader error: {ex.Message}", fileName, ex.Context?.Parser?.Row, ex);
        }
        catch (Exception ex) when (ex is not GtfsParsingException)
        {
            LogUnexpectedError(ex, fileName);
            throw new GtfsParsingException($"Unexpected error parsing {fileName}: {ex.Message}", fileName, null, ex);
        }
    }

    #endregion

    #region Public Parsing Methods

    /// <summary>
    /// Parses stops.txt file from the GTFS directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed <see cref="GtfsStop"/> entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal Task<List<GtfsStop>> ParseStopsAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => ParseCsvFileAsync<GtfsStop>(directoryPath, "stops.txt", progress, cancellationToken);

    /// <summary>
    /// Parses routes.txt file from the GTFS directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed <see cref="GtfsRoute"/> entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal Task<List<GtfsRoute>> ParseRoutesAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => ParseCsvFileAsync<GtfsRoute>(directoryPath, "routes.txt", progress, cancellationToken);

    /// <summary>
    /// Parses trips.txt file from the GTFS directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed <see cref="GtfsTrip"/> entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal Task<List<GtfsTrip>> ParseTripsAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => ParseCsvFileAsync<GtfsTrip>(directoryPath, "trips.txt", progress, cancellationToken);

    /// <summary>
    /// Parses calendar.txt file from the GTFS directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed <see cref="GtfsCalendar"/> entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal Task<List<GtfsCalendar>> ParseCalendarsAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => ParseCsvFileAsync<GtfsCalendar>(directoryPath, "calendar.txt", progress, cancellationToken);

    /// <summary>
    /// Parses stop_times.txt file from the GTFS directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is optimized for handling large files (100,000+ records) using streaming.
    /// Progress is reported at regular intervals (<see cref="ProgressReportInterval"/>) to provide
    /// feedback during long parsing operations.
    /// </para>
    /// <para>
    /// GTFS times are stored as strings to preserve extended time formats (e.g., 25:30:00 for
    /// trips that span past midnight). No time conversion or validation is performed.
    /// </para>
    /// </remarks>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of parsed <see cref="GtfsStopTime"/> entities.</returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal Task<List<GtfsStopTime>> ParseStopTimesAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
        => ParseCsvFileAsync<GtfsStopTime>(directoryPath, "stop_times.txt", progress, cancellationToken);

    /// <summary>
    /// Parses transfers.txt file from the GTFS directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The transfers.txt file is optional in the GTFS specification. If the file does not exist,
    /// this method returns an empty list without throwing an exception.
    /// </para>
    /// <para>
    /// Transfer rules define connections between routes at transfer points, including
    /// minimum transfer times and whether transfers are recommended, timed, or not possible.
    /// </para>
    /// </remarks>
    /// <param name="directoryPath">The path to the directory containing the GTFS files.</param>
    /// <param name="progress">Optional progress reporter for tracking parsing progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A list of parsed <see cref="GtfsTransfer"/> entities, or an empty list if transfers.txt does not exist.
    /// </returns>
    /// <exception cref="GtfsParsingException">Thrown when parsing errors occur (but not for missing file).</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    internal async Task<List<GtfsTransfer>> ParseTransfersAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const string fileName = "transfers.txt";
        var filePath = Path.Combine(directoryPath, fileName);

        // Check if optional file exists - return empty list if not
        if (!File.Exists(filePath))
        {
            LogOptionalFileNotFound(fileName);
            return [];
        }

        return await ParseCsvFileAsync<GtfsTransfer>(directoryPath, fileName, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion
}
