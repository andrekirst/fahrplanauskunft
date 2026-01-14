using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class TimeOfDayTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(12, 30, 750)]
    [InlineData(23, 59, 1439)]
    public void Constructor_WithValidTime_CreatesTimeOfDay(int hours, int minutes, int expectedMinutesSinceMidnight)
    {
        // Act
        var time = new TimeOfDay(hours, minutes);

        // Assert
        time.Hours.Should().Be(hours);
        time.Minutes.Should().Be(minutes);
        time.MinutesSinceMidnight.Should().Be(expectedMinutesSinceMidnight);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 0)]
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

    [Fact]
    public void FromMinutesSinceMidnight_WithValidMinutes_CreatesTimeOfDay()
    {
        // Act
        var time = TimeOfDay.FromMinutesSinceMidnight(750);

        // Assert
        time.Hours.Should().Be(12);
        time.Minutes.Should().Be(30);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1440)]
    [InlineData(2000)]
    public void FromMinutesSinceMidnight_WithInvalidMinutes_ThrowsArgumentOutOfRangeException(int minutes)
    {
        // Act
        var act = () => TimeOfDay.FromMinutesSinceMidnight(minutes);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("minutes");
    }

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
    public void GetHashCode_SameTime_ReturnsSameHashCode()
    {
        // Arrange
        var time1 = new TimeOfDay(12, 30);
        var time2 = new TimeOfDay(12, 30);

        // Act & Assert
        time1.GetHashCode().Should().Be(time2.GetHashCode());
    }
}
