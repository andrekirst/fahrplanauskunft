using FsCheck;
using FsCheck.Xunit;
using Fahrplanauskunft.Core.ValueObjects;

namespace Fahrplanauskunft.Tests.PropertyBased;

/// <summary>
/// Property-based tests for TimeOfDay value object using FsCheck.
/// These tests verify invariants hold for all valid inputs.
/// </summary>
public class TimeOfDayPropertyTests
{
    // Cache generators as static readonly to avoid recreation overhead
    private static readonly Arbitrary<(int hours, int minutes)> ValidHoursAndMinutes = CreateValidHoursAndMinutesArb();
    private static readonly Arbitrary<int> ValidMinutesSinceMidnight = CreateValidMinutesSinceMidnightArb();
    private static readonly Arbitrary<((int h1, int m1), (int h2, int m2))> TwoDistinctTimes = CreateTwoDistinctTimesArb();

    [Property]
    public Property TimeOfDay_ToString_AlwaysReturnsCorrectFormat()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var timeOfDay = new TimeOfDay(hours, minutes);
                var str = timeOfDay.ToString();

                // Property: ToString always returns "HH:MM" format (5 characters with colon at position 2)
                return str.Length == 5 && str[2] == ':';
            });
    }

    [Property]
    public Property TimeOfDay_MinutesSinceMidnight_IsConsistentWithHoursAndMinutes()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var timeOfDay = new TimeOfDay(hours, minutes);

                // Property: MinutesSinceMidnight == Hours * 60 + Minutes
                return timeOfDay.MinutesSinceMidnight == hours * 60 + minutes;
            });
    }

    [Property]
    public Property TimeOfDay_Hours_IsWithinValidRange()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var timeOfDay = new TimeOfDay(hours, minutes);

                // Property: Hours property is always between 0 and 23
                return timeOfDay.Hours >= 0 && timeOfDay.Hours <= 23;
            });
    }

    [Property]
    public Property TimeOfDay_Minutes_IsWithinValidRange()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var timeOfDay = new TimeOfDay(hours, minutes);

                // Property: Minutes property is always between 0 and 59
                return timeOfDay.Minutes >= 0 && timeOfDay.Minutes <= 59;
            });
    }

    [Property]
    public Property TimeOfDay_Equality_IsReflexive()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var timeOfDay = new TimeOfDay(hours, minutes);

                // Property: x == x (reflexive)
                return timeOfDay.Equals(timeOfDay);
            });
    }

    [Property]
    public Property TimeOfDay_Equality_IsSymmetric()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var time1 = new TimeOfDay(hours, minutes);
                var time2 = new TimeOfDay(hours, minutes);

                // Property: x == y implies y == x (symmetric)
                return time1.Equals(time2) == time2.Equals(time1);
            });
    }

    [Property]
    public Property TimeOfDay_CompareTo_IsConsistentWithEquality()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var time1 = new TimeOfDay(hours, minutes);
                var time2 = new TimeOfDay(hours, minutes);

                // Property: CompareTo returns 0 for equal values
                return time1.CompareTo(time2) == 0;
            });
    }

    [Property]
    public Property TimeOfDay_CompareTo_EarlierTimeIsLessThan()
    {
        return Prop.ForAll(
            TwoDistinctTimes,
            tuple =>
            {
                var ((h1, m1), (h2, m2)) = tuple;
                var time1 = new TimeOfDay(h1, m1);
                var time2 = new TimeOfDay(h2, m2);

                var minutes1 = h1 * 60 + m1;
                var minutes2 = h2 * 60 + m2;

                // Property: If minutes1 < minutes2 then time1 < time2
                if (minutes1 < minutes2)
                    return time1.CompareTo(time2) < 0;
                if (minutes1 > minutes2)
                    return time1.CompareTo(time2) > 0;
                return time1.CompareTo(time2) == 0;
            });
    }

    [Property]
    public Property TimeOfDay_FromMinutesSinceMidnight_RoundTrips()
    {
        return Prop.ForAll(
            ValidMinutesSinceMidnight,
            minutes =>
            {
                var timeOfDay = TimeOfDay.FromMinutesSinceMidnight(minutes);

                // Property: Creating from minutes and reading back gives same minutes
                return timeOfDay.MinutesSinceMidnight == minutes;
            });
    }

    [Property]
    public Property TimeOfDay_GetHashCode_IsConsistentWithEquality()
    {
        return Prop.ForAll(
            ValidHoursAndMinutes,
            tuple =>
            {
                var (hours, minutes) = tuple;
                var time1 = new TimeOfDay(hours, minutes);
                var time2 = new TimeOfDay(hours, minutes);

                // Property: Equal objects have equal hash codes
                return time1.GetHashCode() == time2.GetHashCode();
            });
    }

    #region Cached Generator Factories

    private static Arbitrary<(int hours, int minutes)> CreateValidHoursAndMinutesArb()
    {
        var gen = from hours in Gen.Choose(0, 23)
                  from minutes in Gen.Choose(0, 59)
                  select (hours, minutes);
        return Arb.From(gen);
    }

    private static Arbitrary<int> CreateValidMinutesSinceMidnightArb()
    {
        return Arb.From(Gen.Choose(0, 1439)); // 0 to 23:59
    }

    private static Arbitrary<((int h1, int m1), (int h2, int m2))> CreateTwoDistinctTimesArb()
    {
        var gen = from h1 in Gen.Choose(0, 23)
                  from m1 in Gen.Choose(0, 59)
                  from h2 in Gen.Choose(0, 23)
                  from m2 in Gen.Choose(0, 59)
                  select ((h1, m1), (h2, m2));
        return Arb.From(gen);
    }

    #endregion
}
