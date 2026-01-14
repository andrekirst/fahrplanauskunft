using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class TimeOfDayTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(12, 30, 750)]
    [InlineData(23, 59, 1439)]
    public void Constructor_WithValidTime_CreatesTimeOfDay(int hours, int minutes, int expectedTotalMinutes)
    {
        // Act
        var time = new TimeOfDay(hours, minutes);

        // Assert
        time.Hours.Should().Be(hours);
        time.Minutes.Should().Be(minutes);
        time.TotalMinutes.Should().Be(expectedTotalMinutes);
    }

    [Theory]
    [InlineData(24, 0, 1440)]  // Midnight next day
    [InlineData(25, 30, 1530)] // Extended time (1:30 AM next day)
    [InlineData(47, 59, 2879)] // Maximum extended time
    public void Constructor_WithExtendedHours_CreatesTimeOfDay(int hours, int minutes, int expectedTotalMinutes)
    {
        // Act
        var time = new TimeOfDay(hours, minutes);

        // Assert
        time.Hours.Should().Be(hours);
        time.Minutes.Should().Be(minutes);
        time.TotalMinutes.Should().Be(expectedTotalMinutes);
        time.IsExtended.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(48, 0)]
    [InlineData(50, 0)]
    public void Constructor_WithInvalidHours_ThrowsArgumentOutOfRangeException(int hours, int minutes)
    {
        // Act
        var act = () => new TimeOfDay(hours, minutes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("hours");
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 60)]
    [InlineData(0, 100)]
    public void Constructor_WithInvalidMinutes_ThrowsArgumentOutOfRangeException(int hours, int minutes)
    {
        // Act
        var act = () => new TimeOfDay(hours, minutes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minutes");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void FromTotalMinutes_WithValidMinutes_CreatesTimeOfDay()
    {
        // Act
        var time = TimeOfDay.FromTotalMinutes(750);

        // Assert
        time.Hours.Should().Be(12);
        time.Minutes.Should().Be(30);
    }

    [Fact]
    public void FromTotalMinutes_WithExtendedMinutes_CreatesTimeOfDay()
    {
        // Act
        var time = TimeOfDay.FromTotalMinutes(1530); // 25:30

        // Assert
        time.Hours.Should().Be(25);
        time.Minutes.Should().Be(30);
        time.IsExtended.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2880)] // 48 * 60
    [InlineData(3000)]
    public void FromTotalMinutes_WithInvalidMinutes_ThrowsArgumentOutOfRangeException(int minutes)
    {
        // Act
        var act = () => TimeOfDay.FromTotalMinutes(minutes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minutes");
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("08:30", 8, 30)]
    [InlineData("00:00", 0, 0)]
    [InlineData("23:59", 23, 59)]
    [InlineData("25:30", 25, 30)]  // Extended time
    [InlineData("8:05", 8, 5)]     // Single digit hour
    public void Parse_WithValidTimeString_ReturnsTimeOfDay(string timeString, int expectedHours, int expectedMinutes)
    {
        // Act
        var time = TimeOfDay.Parse(timeString);

        // Assert
        time.Hours.Should().Be(expectedHours);
        time.Minutes.Should().Be(expectedMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Parse_WithNullOrWhitespace_ThrowsFormatException(string? timeString)
    {
        // Act
        var act = () => TimeOfDay.Parse(timeString!);

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("12:30:00")]
    [InlineData("12")]
    [InlineData("ab:cd")]
    public void Parse_WithInvalidFormat_ThrowsFormatException(string timeString)
    {
        // Act
        var act = () => TimeOfDay.Parse(timeString);

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("08:30", true)]
    [InlineData("25:30", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryParse_ReturnsExpectedResult(string? timeString, bool expectedResult)
    {
        // Act
        var result = TimeOfDay.TryParse(timeString, out var time);

        // Assert
        result.Should().Be(expectedResult);
        if (expectedResult)
        {
            time.Should().NotBe(default);
        }
    }

    [Theory]
    [InlineData("48:00")]  // Hours out of range
    [InlineData("-1:00")]  // Negative hours
    [InlineData("12:60")]  // Minutes out of range
    [InlineData("12:-1")]  // Negative minutes
    [InlineData("99:99")]  // Both out of range
    public void TryParse_WithOutOfRangeValues_ReturnsFalse(string timeString)
    {
        // Act
        var result = TimeOfDay.TryParse(timeString, out var time);

        // Assert
        result.Should().BeFalse();
        time.Should().Be(default);
    }

    [Fact]
    public void TryParse_ValidExtendedTime_ReturnsExtendedTimeOfDay()
    {
        // Act
        var result = TimeOfDay.TryParse("47:59", out var time);

        // Assert
        result.Should().BeTrue();
        time.Hours.Should().Be(47);
        time.Minutes.Should().Be(59);
        time.IsExtended.Should().BeTrue();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameTime_ReturnsTrue()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        var time2 = new TimeOfDay(12, 30);

        // Act & Assert
        time1.Equals(time2).Should().BeTrue();
        (time1 == time2).Should().BeTrue();
        (time1 != time2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentTime_ReturnsFalse()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        var time2 = new TimeOfDay(12, 31);

        // Act & Assert
        time1.Equals(time2).Should().BeFalse();
        (time1 == time2).Should().BeFalse();
        (time1 != time2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameTime_ReturnsSameHashCode()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        var time2 = new TimeOfDay(12, 30);

        // Act & Assert
        time1.GetHashCode().Should().Be(time2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        var time = new TimeOfDay(12, 30);

        // Act & Assert
        time.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var time = new TimeOfDay(12, 30);
        object differentType = "12:30";

        // Act & Assert
        time.Equals(differentType).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithBoxedTimeOfDay_ReturnsTrue()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        object boxedTime = new TimeOfDay(12, 30);

        // Act & Assert
        time1.Equals(boxedTime).Should().BeTrue();
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_EarlierTime_ReturnsNegative()
    {
        // Arrange
        var earlier = new TimeOfDay(8, 0);
        var later = new TimeOfDay(12, 0);

        // Act & Assert
        earlier.CompareTo(later).Should().BeNegative();
        (earlier < later).Should().BeTrue();
        (earlier <= later).Should().BeTrue();
        (earlier > later).Should().BeFalse();
        (earlier >= later).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_LaterTime_ReturnsPositive()
    {
        // Arrange
        var earlier = new TimeOfDay(8, 0);
        var later = new TimeOfDay(12, 0);

        // Act & Assert
        later.CompareTo(earlier).Should().BePositive();
        (later > earlier).Should().BeTrue();
        (later >= earlier).Should().BeTrue();
        (later < earlier).Should().BeFalse();
        (later <= earlier).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_SameTime_ReturnsZero()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        var time2 = new TimeOfDay(12, 30);

        // Act & Assert
        time1.CompareTo(time2).Should().Be(0);
        (time1 <= time2).Should().BeTrue();
        (time1 >= time2).Should().BeTrue();
    }

    #endregion

    #region Arithmetic Tests

    [Fact]
    public void Add_Duration_ReturnsCorrectTime()
    {
        // Arrange
        var time = new TimeOfDay(8, 30);
        var duration = Duration.FromMinutes(90);

        // Act
        var result = time.Add(duration);

        // Assert
        result.Hours.Should().Be(10);
        result.Minutes.Should().Be(0);
    }

    [Fact]
    public void Add_Duration_AcrossMidnight_ReturnsExtendedTime()
    {
        // Arrange
        var time = new TimeOfDay(23, 30);
        var duration = Duration.FromMinutes(60);

        // Act
        var result = time.Add(duration);

        // Assert
        result.Hours.Should().Be(24);
        result.Minutes.Should().Be(30);
        result.IsExtended.Should().BeTrue();
    }

    [Fact]
    public void Add_Duration_ExceedsMax_ThrowsInvalidOperationException()
    {
        // Arrange
        var time = new TimeOfDay(47, 0);
        var duration = Duration.FromMinutes(120);

        // Act
        var act = () => time.Add(duration);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_Duration_ReturnsCorrectTime()
    {
        // Arrange
        var time = new TimeOfDay(10, 0);
        var duration = Duration.FromMinutes(90);

        // Act
        var result = time.Subtract(duration);

        // Assert
        result.Hours.Should().Be(8);
        result.Minutes.Should().Be(30);
    }

    [Fact]
    public void Subtract_Duration_GoesNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        var time = new TimeOfDay(1, 0);
        var duration = Duration.FromMinutes(90);

        // Act
        var act = () => time.Subtract(duration);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OperatorPlus_AddsDuration()
    {
        // Arrange
        var time = new TimeOfDay(8, 30);
        var duration = Duration.FromMinutes(30);

        // Act
        var result = time + duration;

        // Assert
        result.Hours.Should().Be(9);
        result.Minutes.Should().Be(0);
    }

    [Fact]
    public void OperatorMinus_SubtractsDuration()
    {
        // Arrange
        var time = new TimeOfDay(9, 0);
        var duration = Duration.FromMinutes(30);

        // Act
        var result = time - duration;

        // Assert
        result.Hours.Should().Be(8);
        result.Minutes.Should().Be(30);
    }

    #endregion

    #region Duration Calculation Tests

    [Fact]
    public void DurationUntil_ReturnsAbsoluteDifference()
    {
        // Arrange
        var time1 = new TimeOfDay(8, 0);
        var time2 = new TimeOfDay(10, 30);

        // Act
        var duration = time1.DurationUntil(time2);

        // Assert
        duration.TotalMinutes.Should().Be(150);
    }

    [Fact]
    public void DurationUntil_EarlierTime_ReturnsAbsoluteDifference()
    {
        // Arrange
        var time1 = new TimeOfDay(10, 30);
        var time2 = new TimeOfDay(8, 0);

        // Act
        var duration = time1.DurationUntil(time2);

        // Assert
        duration.TotalMinutes.Should().Be(150);
    }

    [Fact]
    public void ForwardDurationTo_LaterTime_ReturnsDifference()
    {
        // Arrange
        var time1 = new TimeOfDay(8, 0);
        var time2 = new TimeOfDay(10, 30);

        // Act
        var duration = time1.ForwardDurationTo(time2);

        // Assert
        duration.TotalMinutes.Should().Be(150);
    }

    [Fact]
    public void ForwardDurationTo_EarlierTime_HandlesMidnightCrossover()
    {
        // Arrange
        var time1 = new TimeOfDay(23, 0);  // 11 PM
        var time2 = new TimeOfDay(1, 0);   // 1 AM next day

        // Act
        var duration = time1.ForwardDurationTo(time2);

        // Assert
        duration.TotalMinutes.Should().Be(120); // 2 hours across midnight
    }

    #endregion

    #region Normalization Tests

    [Fact]
    public void IsExtended_NormalTime_ReturnsFalse()
    {
        // Arrange
        var time = new TimeOfDay(12, 30);

        // Act & Assert
        time.IsExtended.Should().BeFalse();
    }

    [Fact]
    public void IsExtended_ExtendedTime_ReturnsTrue()
    {
        // Arrange
        var time = new TimeOfDay(25, 30);

        // Act & Assert
        time.IsExtended.Should().BeTrue();
    }

    [Fact]
    public void Normalized_ExtendedTime_ReturnsNormalizedTime()
    {
        // Arrange
        var time = new TimeOfDay(25, 30);

        // Act
        var normalized = time.Normalized;

        // Assert
        normalized.Hours.Should().Be(1);
        normalized.Minutes.Should().Be(30);
        normalized.IsExtended.Should().BeFalse();
    }

    [Fact]
    public void Normalized_NormalTime_ReturnsSameTime()
    {
        // Arrange
        var time = new TimeOfDay(12, 30);

        // Act
        var normalized = time.Normalized;

        // Assert
        normalized.Should().Be(time);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedTime()
    {
        // Arrange
        var time = new TimeOfDay(8, 5);

        // Act
        var result = time.ToString();

        // Assert
        result.Should().Be("08:05");
    }

    [Fact]
    public void ToString_ExtendedTime_ReturnsExtendedFormat()
    {
        // Arrange
        var time = new TimeOfDay(25, 30);

        // Act
        var result = time.ToString();

        // Assert
        result.Should().Be("25:30");
    }

    [Fact]
    public void ToNormalizedString_ExtendedTime_ReturnsNormalizedFormat()
    {
        // Arrange
        var time = new TimeOfDay(25, 30);

        // Act
        var result = time.ToNormalizedString();

        // Assert
        result.Should().Be("01:30");
    }

    #endregion
}
