using CsvHelper.Configuration.Attributes;

namespace Fahrplanauskunft.Infrastructure.Gtfs;

/// <summary>
/// Represents a service schedule from the GTFS calendar.txt file.
/// This is a plain POCO for CsvHelper parsing - no validation or domain logic.
/// See: https://gtfs.org/schedule/reference/#calendartxt
/// </summary>
/// <remarks>
/// Dates are stored as strings in YYYYMMDD format as per GTFS specification.
/// Service days are stored as integers (0 = no service, 1 = service available).
/// </remarks>
public sealed class GtfsCalendar
{
    /// <summary>
    /// Required. Uniquely identifies a set of dates when service is available for one or more routes.
    /// </summary>
    [Name("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Required. Indicates whether the service operates on all Mondays in the date range.
    /// 0 - Service is not available on Mondays.
    /// 1 - Service is available on all Mondays.
    /// </summary>
    [Name("monday")]
    public int Monday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Tuesdays in the date range.
    /// 0 - Service is not available on Tuesdays.
    /// 1 - Service is available on all Tuesdays.
    /// </summary>
    [Name("tuesday")]
    public int Tuesday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Wednesdays in the date range.
    /// 0 - Service is not available on Wednesdays.
    /// 1 - Service is available on all Wednesdays.
    /// </summary>
    [Name("wednesday")]
    public int Wednesday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Thursdays in the date range.
    /// 0 - Service is not available on Thursdays.
    /// 1 - Service is available on all Thursdays.
    /// </summary>
    [Name("thursday")]
    public int Thursday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Fridays in the date range.
    /// 0 - Service is not available on Fridays.
    /// 1 - Service is available on all Fridays.
    /// </summary>
    [Name("friday")]
    public int Friday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Saturdays in the date range.
    /// 0 - Service is not available on Saturdays.
    /// 1 - Service is available on all Saturdays.
    /// </summary>
    [Name("saturday")]
    public int Saturday { get; set; }

    /// <summary>
    /// Required. Indicates whether the service operates on all Sundays in the date range.
    /// 0 - Service is not available on Sundays.
    /// 1 - Service is available on all Sundays.
    /// </summary>
    [Name("sunday")]
    public int Sunday { get; set; }

    /// <summary>
    /// Required. Start service day for the service interval (YYYYMMDD format).
    /// </summary>
    [Name("start_date")]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Required. End service day for the service interval (YYYYMMDD format).
    /// This service day is included in the interval.
    /// </summary>
    [Name("end_date")]
    public string EndDate { get; set; } = string.Empty;
}
