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

    #endregion
}
