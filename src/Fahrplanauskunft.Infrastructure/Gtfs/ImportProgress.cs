namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents progress information during GTFS file parsing.
/// Used with <see cref="IProgress{T}"/> to report parsing progress for large files.
/// </summary>
/// <remarks>
/// Progress is typically reported at regular intervals (e.g., every 10,000 records)
/// to minimize overhead while still providing meaningful feedback for large files
/// like stop_times.txt which can contain 1M+ records.
/// </remarks>
public sealed class ImportProgress
{
    /// <summary>
    /// Gets or sets the name of the GTFS file currently being parsed (e.g., "stops.txt", "stop_times.txt").
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of records processed so far in the current file.
    /// </summary>
    public int RecordsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of records in the current file, if known.
    /// May be null if the total count is not available (e.g., when streaming without pre-counting).
    /// </summary>
    public int? TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the current parsing phase or status message.
    /// Examples: "Reading", "Parsing", "Completed".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets the percentage of records processed (0-100).
    /// Returns null if <see cref="TotalRecords"/> is not set or is zero.
    /// </summary>
    public double? PercentComplete =>
        TotalRecords is > 0
            ? (double)RecordsProcessed / TotalRecords.Value * 100
            : null;

    /// <summary>
    /// Gets a value indicating whether parsing of the current file is complete.
    /// </summary>
    public bool IsComplete =>
        TotalRecords.HasValue && RecordsProcessed >= TotalRecords.Value;

    /// <summary>
    /// Creates a new instance of <see cref="ImportProgress"/> with default values.
    /// </summary>
    public ImportProgress()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ImportProgress"/> with specified values.
    /// </summary>
    /// <param name="currentFile">The name of the file being parsed.</param>
    /// <param name="recordsProcessed">The number of records processed.</param>
    /// <param name="totalRecords">The total number of records, if known.</param>
    /// <param name="status">The current parsing status.</param>
    public ImportProgress(string currentFile, int recordsProcessed, int? totalRecords = null, string status = "Parsing")
    {
        CurrentFile = currentFile;
        RecordsProcessed = recordsProcessed;
        TotalRecords = totalRecords;
        Status = status;
    }

    /// <summary>
    /// Returns a string representation of the current progress.
    /// </summary>
    /// <returns>A formatted string showing the current parsing progress.</returns>
    public override string ToString()
    {
        var progress = PercentComplete.HasValue
            ? $"{PercentComplete:F1}%"
            : $"{RecordsProcessed} records";

        return $"[{Status}] {CurrentFile}: {progress}";
    }
}
