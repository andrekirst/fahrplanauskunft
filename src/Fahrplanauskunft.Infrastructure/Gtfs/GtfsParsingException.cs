namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Exception thrown when errors occur during GTFS file parsing.
/// Provides detailed context about parsing failures including file name, line number, and missing files.
/// </summary>
public class GtfsParsingException : Exception
{
    /// <summary>
    /// Gets the name of the GTFS file where the error occurred (e.g., "stops.txt").
    /// May be null if the error is not related to a specific file.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the line number where the error occurred.
    /// May be null if the error is not related to a specific line.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Gets the list of required files that are missing.
    /// Empty if the error is not related to missing files.
    /// </summary>
    public IReadOnlyList<string> MissingFiles { get; }

    /// <summary>
    /// Creates a new GtfsParsingException with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the parsing failure.</param>
    public GtfsParsingException(string message)
        : base(message)
    {
        MissingFiles = Array.Empty<string>();
    }

    /// <summary>
    /// Creates a new GtfsParsingException with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message describing the parsing failure.</param>
    /// <param name="innerException">The exception that caused this parsing error.</param>
    public GtfsParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
        MissingFiles = Array.Empty<string>();
    }

    /// <summary>
    /// Creates a new GtfsParsingException with file context information.
    /// Use this constructor when an error occurs at a specific location in a GTFS file.
    /// </summary>
    /// <param name="message">The error message describing the parsing failure.</param>
    /// <param name="fileName">The name of the GTFS file where the error occurred.</param>
    /// <param name="lineNumber">The line number where the error occurred.</param>
    /// <param name="innerException">The exception that caused this parsing error, if any.</param>
    public GtfsParsingException(string message, string fileName, int? lineNumber = null, Exception? innerException = null)
        : base(FormatMessageWithContext(message, fileName, lineNumber), innerException)
    {
        FileName = fileName;
        LineNumber = lineNumber;
        MissingFiles = Array.Empty<string>();
    }

    /// <summary>
    /// Creates a new GtfsParsingException for missing required files.
    /// Use this constructor when required GTFS files are not found in the directory.
    /// </summary>
    /// <param name="missingFiles">The list of required file names that are missing.</param>
    public GtfsParsingException(IEnumerable<string> missingFiles)
        : base(FormatMissingFilesMessage(missingFiles))
    {
        MissingFiles = missingFiles.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a new GtfsParsingException for missing required files with an inner exception.
    /// </summary>
    /// <param name="missingFiles">The list of required file names that are missing.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public GtfsParsingException(IEnumerable<string> missingFiles, Exception innerException)
        : base(FormatMissingFilesMessage(missingFiles), innerException)
    {
        MissingFiles = missingFiles.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets a value indicating whether this exception is related to missing required files.
    /// </summary>
    public bool IsMissingFilesError => MissingFiles.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this exception has file context information.
    /// </summary>
    public bool HasFileContext => !string.IsNullOrEmpty(FileName);

    private static string FormatMessageWithContext(string message, string fileName, int? lineNumber)
    {
        if (lineNumber.HasValue)
        {
            return $"Error parsing '{fileName}' at line {lineNumber.Value}: {message}";
        }

        return $"Error parsing '{fileName}': {message}";
    }

    private static string FormatMissingFilesMessage(IEnumerable<string> missingFiles)
    {
        var fileList = missingFiles.ToList();

        if (fileList.Count == 0)
        {
            return "Required GTFS files are missing.";
        }

        if (fileList.Count == 1)
        {
            return $"Required GTFS file is missing: {fileList[0]}";
        }

        return $"Required GTFS files are missing: {string.Join(", ", fileList)}";
    }
}
