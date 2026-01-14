using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class DurationTests
{
    #region Factory Method Tests

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(30, 0, 30)]
    [InlineData(90, 1, 30)]
    [InlineData(150, 2, 30)]
    public void FromMinutes_WithValidMinutes_CreatesDuration(int minutes, int expectedHours, int expectedMinutes)
    {
        // Act
        var duration = Duration.FromMinutes(minutes);

        // Assert
        duration.TotalMinutes.Should().Be(minutes);
        duration.Hours.Should().Be(expectedHours);
        duration.Minutes.Should().Be(expectedMinutes);
    }

    [Fact]
    public void FromMinutes_WithNegativeMinutes_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => Duration.FromMinutes(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("totalMinutes");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 30, 90)]
    [InlineData(2, 0, 120)]
    [InlineData(0, 45, 45)]
    public void FromHoursAndMinutes_WithValidValues_CreatesDuration(int hours, int minutes, int expectedTotalMinutes)
    {
        // Act
        var duration = Duration.FromHoursAndMinutes(hours, minutes);

        // Assert
        duration.TotalMinutes.Should().Be(expectedTotalMinutes);
        duration.Hours.Should().Be(hours);
        duration.Minutes.Should().Be(minutes);
    }

    [Fact]
    public void FromHoursAndMinutes_WithNegativeHours_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => Duration.FromHoursAndMinutes(-1, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("hours");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(100)]
    public void FromHoursAndMinutes_WithInvalidMinutes_ThrowsArgumentOutOfRangeException(int minutes)
    {
        // Act
        var act = () => Duration.FromHoursAndMinutes(0, minutes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minutes");
    }

    #endregion

    #region TimeSpan Conversion Tests

    [Fact]
    public void FromTimeSpan_WithValidTimeSpan_CreatesDuration()
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(90);

        // Act
        var duration = Duration.FromTimeSpan(timeSpan);

        // Assert
        duration.TotalMinutes.Should().Be(90);
    }

    [Fact]
    public void FromTimeSpan_WithNegativeTimeSpan_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(-10);

        // Act
        var act = () => Duration.FromTimeSpan(timeSpan);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("timeSpan");
    }

    [Fact]
    public void ToTimeSpan_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);

        // Act
        var timeSpan = duration.ToTimeSpan();

        // Assert
        timeSpan.Should().Be(TimeSpan.FromMinutes(90));
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Zero_HasZeroMinutes()
    {
        // Act & Assert
        Duration.Zero.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void OneMinute_HasOneMinute()
    {
        // Act & Assert
        Duration.OneMinute.TotalMinutes.Should().Be(1);
    }

    [Fact]
    public void OneHour_HasSixtyMinutes()
    {
        // Act & Assert
        Duration.OneHour.TotalMinutes.Should().Be(60);
    }

    #endregion

    #region Arithmetic Tests

    [Fact]
    public void Add_TwoDurations_ReturnsSum()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(45);

        // Act
        var result = duration1.Add(duration2);

        // Assert
        result.TotalMinutes.Should().Be(75);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 30, 30)]
    [InlineData(30, 0, 30)]
    [InlineData(30, 30, 60)]
    [InlineData(90, 90, 180)]
    [InlineData(1, 59, 60)]
    public void Add_VariousValues_ReturnsCorrectSum(int minutes1, int minutes2, int expectedSum)
    {
        // Arrange
        var duration1 = Duration.FromMinutes(minutes1);
        var duration2 = Duration.FromMinutes(minutes2);

        // Act
        var result = duration1 + duration2;

        // Assert
        result.TotalMinutes.Should().Be(expectedSum);
    }

    [Fact]
    public void Add_ZeroDuration_ReturnsSameValue()
    {
        // Arrange
        var duration = Duration.FromMinutes(45);

        // Act
        var result = duration + Duration.Zero;

        // Assert
        result.Should().Be(duration);
    }

    [Fact]
    public void Add_MultipleChained_ReturnsCorrectSum()
    {
        // Arrange
        var d1 = Duration.FromMinutes(10);
        var d2 = Duration.FromMinutes(20);
        var d3 = Duration.FromMinutes(30);

        // Act
        var result = d1 + d2 + d3;

        // Assert
        result.TotalMinutes.Should().Be(60);
    }

    [Fact]
    public void Subtract_SmallerDuration_ReturnsDifference()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(60);
        var duration2 = Duration.FromMinutes(30);

        // Act
        var result = duration1.Subtract(duration2);

        // Assert
        result.TotalMinutes.Should().Be(30);
    }

    [Theory]
    [InlineData(60, 30, 30)]
    [InlineData(90, 45, 45)]
    [InlineData(120, 60, 60)]
    [InlineData(100, 1, 99)]
    [InlineData(60, 0, 60)]
    public void Subtract_VariousValues_ReturnsCorrectDifference(int minutes1, int minutes2, int expectedDifference)
    {
        // Arrange
        var duration1 = Duration.FromMinutes(minutes1);
        var duration2 = Duration.FromMinutes(minutes2);

        // Act
        var result = duration1 - duration2;

        // Assert
        result.TotalMinutes.Should().Be(expectedDifference);
    }

    [Fact]
    public void Subtract_EqualDurations_ReturnsZero()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(45);
        var duration2 = Duration.FromMinutes(45);

        // Act
        var result = duration1 - duration2;

        // Assert
        result.Should().Be(Duration.Zero);
    }

    [Fact]
    public void Subtract_ZeroDuration_ReturnsSameValue()
    {
        // Arrange
        var duration = Duration.FromMinutes(45);

        // Act
        var result = duration - Duration.Zero;

        // Assert
        result.Should().Be(duration);
    }

    [Fact]
    public void Subtract_LargerDuration_ThrowsInvalidOperationException()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(60);

        // Act
        var act = () => duration1.Subtract(duration2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(30, 31)]
    [InlineData(0, 1)]
    [InlineData(59, 60)]
    public void Subtract_LargerDuration_VariousValues_ThrowsInvalidOperationException(int minutes1, int minutes2)
    {
        // Arrange
        var duration1 = Duration.FromMinutes(minutes1);
        var duration2 = Duration.FromMinutes(minutes2);

        // Act
        var act = () => duration1 - duration2;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_ByPositiveFactor_ReturnsMultipliedDuration()
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        var result = duration.Multiply(3);

        // Assert
        result.TotalMinutes.Should().Be(90);
    }

    [Theory]
    [InlineData(10, 0, 0)]
    [InlineData(10, 1, 10)]
    [InlineData(10, 2, 20)]
    [InlineData(15, 4, 60)]
    [InlineData(1, 100, 100)]
    [InlineData(60, 24, 1440)]
    public void Multiply_VariousValues_ReturnsCorrectProduct(int minutes, int factor, int expectedProduct)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act
        var result = duration * factor;

        // Assert
        result.TotalMinutes.Should().Be(expectedProduct);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZeroDuration()
    {
        // Arrange
        var duration = Duration.FromMinutes(100);

        // Act
        var result = duration * 0;

        // Assert
        result.Should().Be(Duration.Zero);
    }

    [Fact]
    public void Multiply_ZeroDuration_ReturnsZero()
    {
        // Arrange
        var duration = Duration.Zero;

        // Act
        var result = duration * 100;

        // Assert
        result.Should().Be(Duration.Zero);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        var act = () => duration.Multiply(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("factor");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Multiply_ByNegativeFactor_VariousValues_ThrowsArgumentOutOfRangeException(int factor)
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        var act = () => duration * factor;

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("factor");
    }

    [Fact]
    public void OperatorPlus_AddsDurations()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(45);

        // Act
        var result = duration1 + duration2;

        // Assert
        result.TotalMinutes.Should().Be(75);
    }

    [Fact]
    public void OperatorMinus_SubtractsDurations()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(60);
        var duration2 = Duration.FromMinutes(30);

        // Act
        var result = duration1 - duration2;

        // Assert
        result.TotalMinutes.Should().Be(30);
    }

    [Fact]
    public void OperatorMinus_LargerFromSmaller_ThrowsInvalidOperationException()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(30);
        var duration2 = Duration.FromMinutes(60);

        // Act
        var act = () => duration1 - duration2;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OperatorMultiply_DurationTimesInt_ReturnsMultipliedDuration()
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        var result = duration * 3;

        // Assert
        result.TotalMinutes.Should().Be(90);
    }

    [Fact]
    public void OperatorMultiply_IntTimesDuration_ReturnsMultipliedDuration()
    {
        // Arrange
        var duration = Duration.FromMinutes(30);

        // Act
        var result = 3 * duration;

        // Assert
        result.TotalMinutes.Should().Be(90);
    }

    [Fact]
    public void OperatorMultiply_Commutative_BothOrdersReturnSameResult()
    {
        // Arrange
        var duration = Duration.FromMinutes(15);
        var factor = 5;

        // Act
        var result1 = duration * factor;
        var result2 = factor * duration;

        // Assert
        result1.Should().Be(result2);
        result1.TotalMinutes.Should().Be(75);
    }

    [Fact]
    public void ChainedOperations_AddSubtractMultiply_ReturnsCorrectResult()
    {
        // Arrange
        var d1 = Duration.FromMinutes(20);
        var d2 = Duration.FromMinutes(10);

        // Act: (20 + 10) * 2 - 10 = 50
        var result = (d1 + d2) * 2 - d2;

        // Assert
        result.TotalMinutes.Should().Be(50);
    }

    [Fact]
    public void Arithmetic_LargeDurations_WorksCorrectly()
    {
        // Arrange - 24 hours
        var oneDay = Duration.FromMinutes(1440);
        var twelveHours = Duration.FromMinutes(720);

        // Act
        var sum = oneDay + twelveHours;
        var difference = oneDay - twelveHours;

        // Assert
        sum.TotalMinutes.Should().Be(2160); // 36 hours
        sum.Hours.Should().Be(36);
        sum.Minutes.Should().Be(0);

        difference.TotalMinutes.Should().Be(720); // 12 hours
        difference.Hours.Should().Be(12);
        difference.Minutes.Should().Be(0);
    }

    #endregion

    #region TotalSeconds Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 60)]
    [InlineData(30, 1800)]
    [InlineData(60, 3600)]
    [InlineData(90, 5400)]
    [InlineData(1440, 86400)] // 24 hours
    public void TotalSeconds_ReturnsCorrectValue(int minutes, int expectedSeconds)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act & Assert
        duration.TotalSeconds.Should().Be(expectedSeconds);
    }

    [Fact]
    public void TotalSeconds_Zero_ReturnsZero()
    {
        // Arrange & Act
        var duration = Duration.Zero;

        // Assert
        duration.TotalSeconds.Should().Be(0);
    }

    [Fact]
    public void TotalSeconds_OneHour_Returns3600()
    {
        // Arrange & Act
        var duration = Duration.OneHour;

        // Assert
        duration.TotalSeconds.Should().Be(3600);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameDuration_ReturnsTrue()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(90);
        var duration2 = Duration.FromMinutes(90);

        // Act & Assert
        duration1.Equals(duration2).Should().BeTrue();
        (duration1 == duration2).Should().BeTrue();
        (duration1 != duration2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentDuration_ReturnsFalse()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(90);
        var duration2 = Duration.FromMinutes(60);

        // Act & Assert
        duration1.Equals(duration2).Should().BeFalse();
        (duration1 == duration2).Should().BeFalse();
        (duration1 != duration2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameDuration_ReturnsSameHashCode()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(90);
        var duration2 = Duration.FromMinutes(90);

        // Act & Assert
        duration1.GetHashCode().Should().Be(duration2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);

        // Act & Assert
        duration.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);
        object differentType = "1h 30m";

        // Act & Assert
        duration.Equals(differentType).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithBoxedDuration_ReturnsTrue()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(90);
        object boxedDuration = Duration.FromMinutes(90);

        // Act & Assert
        duration1.Equals(boxedDuration).Should().BeTrue();
    }

    [Fact]
    public void Default_Duration_HasZeroMinutes()
    {
        // Arrange
        var duration = default(Duration);

        // Assert
        duration.TotalMinutes.Should().Be(0);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_ShorterDuration_ReturnsPositive()
    {
        // Arrange
        var longer = Duration.FromMinutes(90);
        var shorter = Duration.FromMinutes(60);

        // Act & Assert
        longer.CompareTo(shorter).Should().BePositive();
        (longer > shorter).Should().BeTrue();
        (longer >= shorter).Should().BeTrue();
        (longer < shorter).Should().BeFalse();
        (longer <= shorter).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_LongerDuration_ReturnsNegative()
    {
        // Arrange
        var shorter = Duration.FromMinutes(60);
        var longer = Duration.FromMinutes(90);

        // Act & Assert
        shorter.CompareTo(longer).Should().BeNegative();
        (shorter < longer).Should().BeTrue();
        (shorter <= longer).Should().BeTrue();
        (shorter > longer).Should().BeFalse();
        (shorter >= longer).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_SameDuration_ReturnsZero()
    {
        // Arrange
        var duration1 = Duration.FromMinutes(90);
        var duration2 = Duration.FromMinutes(90);

        // Act & Assert
        duration1.CompareTo(duration2).Should().Be(0);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ZeroDuration_ReturnsZeroMinutes()
    {
        // Arrange
        var duration = Duration.Zero;

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be("0m");
    }

    [Fact]
    public void ToString_MinutesOnly_ReturnsMinutesFormat()
    {
        // Arrange
        var duration = Duration.FromMinutes(45);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be("45m");
    }

    [Fact]
    public void ToString_HoursOnly_ReturnsHoursFormat()
    {
        // Arrange
        var duration = Duration.FromMinutes(120);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be("2h");
    }

    [Fact]
    public void ToString_HoursAndMinutes_ReturnsFullFormat()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be("1h 30m");
    }

    [Fact]
    public void ToTimeString_ReturnsHHMMFormat()
    {
        // Arrange
        var duration = Duration.FromMinutes(90);

        // Act
        var result = duration.ToTimeString();

        // Assert
        result.Should().Be("01:30");
    }

    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(1, "00:01")]
    [InlineData(59, "00:59")]
    [InlineData(60, "01:00")]
    [InlineData(61, "01:01")]
    [InlineData(119, "01:59")]
    [InlineData(120, "02:00")]
    [InlineData(600, "10:00")]
    [InlineData(1440, "24:00")]
    public void ToTimeString_VariousValues_ReturnsCorrectFormat(int minutes, string expected)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act
        var result = duration.ToTimeString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, "5m")]
    [InlineData(9, "9m")]
    [InlineData(10, "10m")]
    [InlineData(59, "59m")]
    public void ToString_SingleAndDoubleDigitMinutes_ReturnsCorrectFormat(int minutes, string expected)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(60, "1h")]
    [InlineData(120, "2h")]
    [InlineData(540, "9h")]
    [InlineData(600, "10h")]
    [InlineData(5940, "99h")]
    [InlineData(6000, "100h")]
    public void ToString_HoursOnly_VariousValues_ReturnsCorrectFormat(int minutes, string expected)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(61, "1h 1m")]
    [InlineData(90, "1h 30m")]
    [InlineData(150, "2h 30m")]
    [InlineData(605, "10h 5m")]
    [InlineData(1439, "23h 59m")]
    public void ToString_HoursAndMinutes_VariousValues_ReturnsCorrectFormat(int minutes, string expected)
    {
        // Arrange
        var duration = Duration.FromMinutes(minutes);

        // Act
        var result = duration.ToString();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Transit Schedule Scenario Tests

    [Fact]
    public void TransitScenario_TypicalBusRideTime()
    {
        // Arrange - typical bus ride of 25 minutes
        var rideTime = Duration.FromMinutes(25);

        // Assert
        rideTime.Hours.Should().Be(0);
        rideTime.Minutes.Should().Be(25);
        rideTime.ToString().Should().Be("25m");
    }

    [Fact]
    public void TransitScenario_LongDistanceTrainRide()
    {
        // Arrange - long distance train: 3 hours 45 minutes
        var rideTime = Duration.FromHoursAndMinutes(3, 45);

        // Assert
        rideTime.TotalMinutes.Should().Be(225);
        rideTime.Hours.Should().Be(3);
        rideTime.Minutes.Should().Be(45);
        rideTime.ToString().Should().Be("3h 45m");
    }

    [Fact]
    public void TransitScenario_MultipleConnectionsAddUp()
    {
        // Arrange - multiple transit legs
        var firstLeg = Duration.FromMinutes(15);     // Bus to station
        var waitTime = Duration.FromMinutes(10);     // Wait for train
        var trainRide = Duration.FromMinutes(45);    // Train ride
        var walkTime = Duration.FromMinutes(5);      // Walk to destination

        // Act
        var totalDuration = firstLeg + waitTime + trainRide + walkTime;

        // Assert
        totalDuration.TotalMinutes.Should().Be(75);
        totalDuration.Hours.Should().Be(1);
        totalDuration.Minutes.Should().Be(15);
    }

    [Fact]
    public void TransitScenario_FrequencyBasedService()
    {
        // Arrange - bus runs every 15 minutes
        var frequency = Duration.FromMinutes(15);

        // Act - calculate time for 4th departure
        var thirdDepartureAfterFirst = frequency * 3;

        // Assert
        thirdDepartureAfterFirst.TotalMinutes.Should().Be(45);
    }

    [Fact]
    public void TransitScenario_DelayCalculation()
    {
        // Arrange
        var scheduledDuration = Duration.FromMinutes(30);
        var actualDuration = Duration.FromMinutes(45);

        // Act
        var delay = actualDuration - scheduledDuration;

        // Assert
        delay.TotalMinutes.Should().Be(15);
        delay.ToString().Should().Be("15m");
    }

    [Fact]
    public void TransitScenario_BufferTimeForConnections()
    {
        // Arrange - train arrives, need minimum 5 min to catch connection
        var minimumTransferTime = Duration.FromMinutes(5);
        var bufferMultiplier = 2; // Safety buffer

        // Act
        var recommendedTransferTime = minimumTransferTime * bufferMultiplier;

        // Assert
        recommendedTransferTime.TotalMinutes.Should().Be(10);
    }

    [Fact]
    public void TransitScenario_DailyServiceDuration()
    {
        // Arrange - service runs from 5:00 to 24:00 (19 hours)
        var dailyServiceHours = Duration.FromHoursAndMinutes(19, 0);

        // Assert
        dailyServiceHours.TotalMinutes.Should().Be(1140);
        dailyServiceHours.Hours.Should().Be(19);
        dailyServiceHours.Minutes.Should().Be(0);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void EdgeCase_VeryLargeDuration()
    {
        // Arrange - 1000 hours
        var largeDuration = Duration.FromHoursAndMinutes(1000, 30);

        // Assert
        largeDuration.TotalMinutes.Should().Be(60030);
        largeDuration.Hours.Should().Be(1000);
        largeDuration.Minutes.Should().Be(30);
    }

    [Fact]
    public void EdgeCase_FromTimeSpan_WithSeconds_TruncatesToMinutes()
    {
        // Arrange - TimeSpan with 45 seconds (should be truncated to 0 minutes)
        var timeSpanWithSeconds = TimeSpan.FromSeconds(45);

        // Act
        var duration = Duration.FromTimeSpan(timeSpanWithSeconds);

        // Assert - 45 seconds = 0 complete minutes
        duration.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void EdgeCase_FromTimeSpan_With90Seconds_Returns1Minute()
    {
        // Arrange - TimeSpan with 90 seconds
        var timeSpanWithSeconds = TimeSpan.FromSeconds(90);

        // Act
        var duration = Duration.FromTimeSpan(timeSpanWithSeconds);

        // Assert - 90 seconds = 1 complete minute (truncated)
        duration.TotalMinutes.Should().Be(1);
    }

    [Fact]
    public void EdgeCase_RoundTripTimeSpanConversion()
    {
        // Arrange
        var originalDuration = Duration.FromMinutes(135);

        // Act
        var timeSpan = originalDuration.ToTimeSpan();
        var roundTripped = Duration.FromTimeSpan(timeSpan);

        // Assert
        roundTripped.Should().Be(originalDuration);
    }

    [Fact]
    public void EdgeCase_Default_EqualsZero()
    {
        // Arrange
        var defaultDuration = default(Duration);

        // Assert
        defaultDuration.Should().Be(Duration.Zero);
        defaultDuration.TotalMinutes.Should().Be(0);
    }

    [Fact]
    public void EdgeCase_OneMinute_Components()
    {
        // Arrange
        var oneMinute = Duration.OneMinute;

        // Assert
        oneMinute.TotalMinutes.Should().Be(1);
        oneMinute.Hours.Should().Be(0);
        oneMinute.Minutes.Should().Be(1);
        oneMinute.TotalSeconds.Should().Be(60);
    }

    [Fact]
    public void EdgeCase_OneHour_Components()
    {
        // Arrange
        var oneHour = Duration.OneHour;

        // Assert
        oneHour.TotalMinutes.Should().Be(60);
        oneHour.Hours.Should().Be(1);
        oneHour.Minutes.Should().Be(0);
        oneHour.TotalSeconds.Should().Be(3600);
    }

    [Fact]
    public void EdgeCase_59Minutes_IsNotOneHour()
    {
        // Arrange
        var fiftyNineMinutes = Duration.FromMinutes(59);

        // Assert
        fiftyNineMinutes.Hours.Should().Be(0);
        fiftyNineMinutes.Minutes.Should().Be(59);
        fiftyNineMinutes.Should().BeLessThan(Duration.OneHour);
    }

    [Fact]
    public void EdgeCase_BoundaryAt60Minutes()
    {
        // Arrange
        var sixtyMinutes = Duration.FromMinutes(60);

        // Assert
        sixtyMinutes.Hours.Should().Be(1);
        sixtyMinutes.Minutes.Should().Be(0);
        sixtyMinutes.Should().Be(Duration.OneHour);
    }

    [Fact]
    public void EdgeCase_MaxInt32Minutes_DoesNotOverflow()
    {
        // This test verifies the duration can handle large values
        // Note: In practice, we wouldn't use such large values, but the type should handle it
        var largeMinutes = int.MaxValue / 2; // Use half to avoid overflow in TotalSeconds

        // Act - should not throw
        var largeDuration = Duration.FromMinutes(largeMinutes);

        // Assert
        largeDuration.TotalMinutes.Should().Be(largeMinutes);
    }

    #endregion
}
