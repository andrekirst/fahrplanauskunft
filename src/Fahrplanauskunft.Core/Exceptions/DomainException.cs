namespace Fahrplanauskunft.Core.Exceptions;

/// <summary>
/// Base exception class for all domain-specific exceptions.
/// Used to signal business rule violations and domain invariant failures.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// The code identifying the type of domain error.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Creates a new DomainException with the specified message.
    /// </summary>
    /// <param name="message">The error message</param>
    public DomainException(string message)
        : base(message)
    {
        ErrorCode = "DOMAIN_ERROR";
    }

    /// <summary>
    /// Creates a new DomainException with the specified message and error code.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code identifying the type of error</param>
    public DomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode ?? "DOMAIN_ERROR";
    }

    /// <summary>
    /// Creates a new DomainException with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "DOMAIN_ERROR";
    }

    /// <summary>
    /// Creates a new DomainException with the specified message, error code, and inner exception.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code identifying the type of error</param>
    /// <param name="innerException">The inner exception</param>
    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? "DOMAIN_ERROR";
    }
}

/// <summary>
/// Exception thrown when a duplicate sequence number is detected in a trip.
/// </summary>
public class DuplicateSequenceException : DomainException
{
    public int DuplicateSequenceNumber { get; }

    public DuplicateSequenceException(int sequenceNumber)
        : base($"Duplicate stop sequence number detected: {sequenceNumber}.", "DUPLICATE_SEQUENCE")
    {
        DuplicateSequenceNumber = sequenceNumber;
    }
}

/// <summary>
/// Exception thrown when arrival time is after departure time at a stop.
/// </summary>
public class InvalidStopTimeException : DomainException
{
    public InvalidStopTimeException(string message)
        : base(message, "INVALID_STOP_TIME")
    {
    }
}

/// <summary>
/// Exception thrown when a footpath has the same origin and destination (self-loop).
/// </summary>
public class SelfLoopException : DomainException
{
    public SelfLoopException()
        : base("A footpath cannot have the same origin and destination stop.", "SELF_LOOP")
    {
    }
}
