using FluentAssertions;
using Xunit;

namespace ModelingEvolution.Numeric.Tests;

public class MovingAverageTests
{
    [Fact]
    public void Constructor_WithZeroWindowSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new MovingAverage<double>(0));
    }

    [Fact]
    public void Constructor_WithNegativeWindowSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new MovingAverage<double>(-1));
    }

    [Fact]
    public void NewInstance_HasZeroCountAndAverage()
    {
        // Arrange & Act
        var avg = new MovingAverage<double>(5);

        // Assert
        avg.Count.Should().Be(0);
        avg.Capacity.Should().Be(5);
        avg.Average.Should().Be(0.0);
    }

    [Fact]
    public void Add_SingleValue_UpdatesAverageAndCount()
    {
        // Arrange
        var avg = new MovingAverage<double>(5);

        // Act
        avg.Add(10.0);

        // Assert
        avg.Count.Should().Be(1);
        avg.Average.Should().Be(10.0);
    }

    [Fact]
    public void Add_MultipleValues_CalculatesCorrectAverage()
    {
        // Arrange
        var avg = new MovingAverage<double>(5);

        // Act
        avg.Add(10.0);
        avg.Add(20.0);
        avg.Add(30.0);

        // Assert
        avg.Count.Should().Be(3);
        avg.Average.Should().Be(20.0); // (10 + 20 + 30) / 3 = 20
    }

    [Fact]
    public void Add_MoreValuesThanCapacity_MaintainsWindowSize()
    {
        // Arrange
        var avg = new MovingAverage<double>(3);

        // Act
        avg.Add(10.0); // [10]
        avg.Add(20.0); // [10, 20]
        avg.Add(30.0); // [10, 20, 30]
        avg.Add(40.0); // [20, 30, 40] - 10 is removed

        // Assert
        avg.Count.Should().Be(3);
        avg.Average.Should().Be(30.0); // (20 + 30 + 40) / 3 = 30
    }

    [Fact]
    public void Add_CircularBufferRollover_CalculatesCorrectAverage()
    {
        // Arrange
        var avg = new MovingAverage<double>(3);

        // Act
        avg.Add(10.0); // [10]
        avg.Add(20.0); // [10, 20]
        avg.Add(30.0); // [10, 20, 30]
        avg.Add(40.0); // [20, 30, 40]
        avg.Add(50.0); // [30, 40, 50]
        avg.Add(60.0); // [40, 50, 60]

        // Assert
        avg.Count.Should().Be(3);
        avg.Average.Should().Be(50.0); // (40 + 50 + 60) / 3 = 50
    }

    [Fact]
    public void Clear_ResetsToInitialState()
    {
        // Arrange
        var avg = new MovingAverage<double>(5);
        avg.Add(10.0);
        avg.Add(20.0);
        avg.Add(30.0);

        // Act
        avg.Clear();

        // Assert
        avg.Count.Should().Be(0);
        avg.Average.Should().Be(0.0);
    }

    [Fact]
    public void OperatorPlus_AddsValueAndReturnsUpdatedStruct()
    {
        // Arrange
        var avg = new MovingAverage<double>(3);

        // Act
        avg += 10.0;
        avg += 20.0;
        avg += 30.0;

        // Assert
        avg.Count.Should().Be(3);
        avg.Average.Should().Be(20.0);
    }

    [Fact]
    public void ImplicitConversion_ReturnsAverage()
    {
        // Arrange
        var avg = new MovingAverage<double>(3);
        avg.Add(10.0);
        avg.Add(20.0);
        avg.Add(30.0);

        // Act
        double result = avg;

        // Assert
        result.Should().Be(20.0);
    }

    [Fact]
    public void WorksWithFloat()
    {
        // Arrange
        var avg = new MovingAverage<float>(3);

        // Act
        avg.Add(10.5f);
        avg.Add(20.5f);
        avg.Add(30.5f);

        // Assert
        avg.Average.Should().BeApproximately(20.5f, 0.001f);
    }

    [Fact]
    public void WorksWithDecimal()
    {
        // Arrange
        var avg = new MovingAverage<decimal>(3);

        // Act
        avg.Add(10.5m);
        avg.Add(20.5m);
        avg.Add(30.5m);

        // Assert
        avg.Average.Should().Be(20.5m);
    }

    [Fact]
    public void FpsScenario_CalculatesCorrectAverage()
    {
        // Arrange - Simulate FPS calculation from frame intervals
        var avg = new MovingAverage<double>(5);

        // Act - Add frame intervals in seconds
        // 30 fps = ~0.0333s per frame
        avg += 0.0333;
        avg += 0.0334;
        avg += 0.0332;
        avg += 0.0333;
        avg += 0.0334;

        // Assert
        double avgInterval = avg;
        double fps = 1.0 / avgInterval;
        fps.Should().BeApproximately(30.0, 0.5); // ~30 fps
    }

    [Fact]
    public void ToString_ReturnsAverageAsString()
    {
        // Arrange
        var avg = new MovingAverage<double>(3);
        avg.Add(10.0);
        avg.Add(20.0);
        avg.Add(30.0);

        // Act
        string result = avg.ToString();

        // Assert
        result.Should().Be("20");
    }
}
