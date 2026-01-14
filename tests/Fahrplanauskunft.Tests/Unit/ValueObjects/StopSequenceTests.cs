using Fahrplanauskunft.Core.ValueObjects;
using FluentAssertions;

namespace Fahrplanauskunft.Tests.Unit.ValueObjects;

public class StopSequenceTests
{
    #region Constructor Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Constructor_WithValidValue_CreatesStopSequence(int value)
    {
        // Act
        var sequence = new StopSequence(value);

        // Assert
        sequence.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithValueBelowMinimum_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act
        var act = () => new StopSequence(value);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void Constructor_WithValueAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => new StopSequence(10001);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void From_WithValidValue_CreatesStopSequence()
    {
        // Act
        var sequence = StopSequence.From(5);

        // Assert
        sequence.Value.Should().Be(5);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = StopSequence.TryCreate(5, out var sequence);

        // Assert
        result.Should().BeTrue();
        sequence.Value.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10001)]
    public void TryCreate_WithInvalidValue_ReturnsFalse(int value)
    {
        // Act
        var result = StopSequence.TryCreate(value, out var sequence);

        // Assert
        result.Should().BeFalse();
        sequence.Should().Be(default);
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void First_ReturnsSequenceWithValueOne()
    {
        // Act
        var first = StopSequence.First;

        // Assert
        first.Value.Should().Be(1);
    }

    #endregion

    #region IsFirst Tests

    [Fact]
    public void IsFirst_WhenValueIsOne_ReturnsTrue()
    {
        // Arrange
        var sequence = new StopSequence(1);

        // Act & Assert
        sequence.IsFirst.Should().BeTrue();
    }

    [Fact]
    public void IsFirst_WhenValueIsNotOne_ReturnsFalse()
    {
        // Arrange
        var sequence = new StopSequence(2);

        // Act & Assert
        sequence.IsFirst.Should().BeFalse();
    }

    #endregion

    #region Next/Previous Tests

    [Fact]
    public void Next_ReturnsNextSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var next = sequence.Next();

        // Assert
        next.Value.Should().Be(6);
    }

    [Fact]
    public void Next_AtMaximum_ThrowsInvalidOperationException()
    {
        // Arrange
        var sequence = new StopSequence(10000);

        // Act
        var act = () => sequence.Next();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Previous_ReturnsPreviousSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var previous = sequence.Previous();

        // Assert
        previous.Value.Should().Be(4);
    }

    [Fact]
    public void Previous_AtMinimum_ThrowsInvalidOperationException()
    {
        // Arrange
        var sequence = new StopSequence(1);

        // Act
        var act = () => sequence.Previous();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OperatorIncrement_ReturnsNextSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var next = ++sequence;

        // Assert
        next.Value.Should().Be(6);
    }

    [Fact]
    public void OperatorDecrement_ReturnsPreviousSequence()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var previous = --sequence;

        // Assert
        previous.Value.Should().Be(4);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.Equals(sequence2).Should().BeTrue();
        (sequence1 == sequence2).Should().BeTrue();
        (sequence1 != sequence2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(6);

        // Act & Assert
        sequence1.Equals(sequence2).Should().BeFalse();
        (sequence1 == sequence2).Should().BeFalse();
        (sequence1 != sequence2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.GetHashCode().Should().Be(sequence2.GetHashCode());
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_LowerSequence_ReturnsPositive()
    {
        // Arrange
        var higher = new StopSequence(10);
        var lower = new StopSequence(5);

        // Act & Assert
        higher.CompareTo(lower).Should().BePositive();
        (higher > lower).Should().BeTrue();
        (higher >= lower).Should().BeTrue();
        (higher < lower).Should().BeFalse();
        (higher <= lower).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_HigherSequence_ReturnsNegative()
    {
        // Arrange
        var lower = new StopSequence(5);
        var higher = new StopSequence(10);

        // Act & Assert
        lower.CompareTo(higher).Should().BeNegative();
        (lower < higher).Should().BeTrue();
        (lower <= higher).Should().BeTrue();
        (lower > higher).Should().BeFalse();
        (lower >= higher).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_SameSequence_ReturnsZero()
    {
        // Arrange
        var sequence1 = new StopSequence(5);
        var sequence2 = new StopSequence(5);

        // Act & Assert
        sequence1.CompareTo(sequence2).Should().Be(0);
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitToInt_ReturnsValue()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var result = (int)sequence;

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void ExplicitFromInt_CreatesStopSequence()
    {
        // Act
        var sequence = (StopSequence)5;

        // Assert
        sequence.Value.Should().Be(5);
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        // Arrange
        var sequence = new StopSequence(5);

        // Act
        var result = sequence.ToString();

        // Assert
        result.Should().Be("5");
    }

    #endregion
}
