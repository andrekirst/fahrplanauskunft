namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Defines the contract for parsing GTFS (General Transit Feed Specification) CSV files.
/// Implementations read GTFS data from a directory containing CSV files and return
/// a populated <see cref="GtfsDataSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// GTFS is a standardized format for public transit schedules and associated geographic information.
/// See: https://gtfs.org/schedule/reference/
/// </para>
/// <para>
/// Required GTFS files (must exist in the directory):
/// <list type="bullet">
///   <item><description>stops.txt - Stop/station locations</description></item>
///   <item><description>routes.txt - Transit routes</description></item>
///   <item><description>trips.txt - Trips for each route</description></item>
///   <item><description>stop_times.txt - Times that a vehicle arrives/departs stops</description></item>
///   <item><description>calendar.txt - Service dates</description></item>
/// </list>
/// </para>
/// <para>
/// Optional GTFS files:
/// <list type="bullet">
///   <item><description>transfers.txt - Transfer rules between stops</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IGtfsParser
{
    /// <summary>
    /// Parses all GTFS CSV files from the specified directory asynchronously.
    /// </summary>
    /// <param name="directoryPath">
    /// The path to the directory containing GTFS CSV files.
    /// The directory must exist and contain all required GTFS files.
    /// </param>
    /// <param name="progress">
    /// Optional progress reporter for tracking parsing progress.
    /// Progress is reported at regular intervals during parsing of large files.
    /// Pass <c>null</c> if progress reporting is not needed.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the parsing operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous parsing operation.
    /// The task result contains a <see cref="GtfsDataSet"/> with all parsed GTFS entities.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when <paramref name="directoryPath"/> does not exist.
    /// </exception>
    /// <exception cref="GtfsParsingException">
    /// Thrown when required GTFS files are missing or when parsing errors occur.
    /// Check <see cref="GtfsParsingException.MissingFiles"/> for missing file details
    /// or <see cref="GtfsParsingException.FileName"/> and <see cref="GtfsParsingException.LineNumber"/>
    /// for parse error context.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    Task<GtfsDataSet> ParseAsync(
        string directoryPath,
        IProgress<ImportProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
