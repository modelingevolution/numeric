using FluentAssertions;
using Xunit;

namespace ModelingEvolution.Numeric.Tests;

public class MovingMedianTests
{
    [Fact]
    public void Constructor_WithZeroWindowSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MovingMedian<double>(0));
    }

    [Fact]
    public void Constructor_WithNegativeWindowSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MovingMedian<double>(-1));
    }

    [Fact]
    public void NewInstance_HasZeroCountAndMedian()
    {
        var m = new MovingMedian<double>(5);

        m.Count.Should().Be(0);
        m.Capacity.Should().Be(5);
        m.Median.Should().Be(0.0);
    }

    [Fact]
    public void Add_SingleValue_MedianIsThatValue()
    {
        var m = new MovingMedian<double>(5);
        m.Add(42.0);

        m.Count.Should().Be(1);
        m.Median.Should().Be(42.0);
    }

    [Fact]
    public void OddCount_ReturnsMiddleValue()
    {
        var m = new MovingMedian<double>(5);
        m.Add(10.0);
        m.Add(30.0);
        m.Add(20.0);

        // sorted: [10, 20, 30] → median = 20
        m.Median.Should().Be(20.0);
    }

    [Fact]
    public void EvenCount_ReturnsAverageOfTwoMiddle()
    {
        var m = new MovingMedian<double>(5);
        m.Add(10.0);
        m.Add(30.0);
        m.Add(20.0);
        m.Add(40.0);

        // sorted: [10, 20, 30, 40] → median = (20+30)/2 = 25
        m.Median.Should().Be(25.0);
    }

    [Fact]
    public void WindowSlides_OldestDropsOut()
    {
        var m = new MovingMedian<double>(3);
        m.Add(100.0); // [100]
        m.Add(1.0);   // [100, 1]
        m.Add(2.0);   // [100, 1, 2]
        m.Add(3.0);   // [1, 2, 3] — 100 dropped

        // sorted: [1, 2, 3] → median = 2
        m.Median.Should().Be(2.0);
    }

    [Fact]
    public void CircularBufferRollover_CalculatesCorrectMedian()
    {
        var m = new MovingMedian<double>(3);
        m.Add(10.0); // [10]
        m.Add(20.0); // [10, 20]
        m.Add(30.0); // [10, 20, 30]
        m.Add(40.0); // [20, 30, 40]
        m.Add(50.0); // [30, 40, 50]
        m.Add(60.0); // [40, 50, 60]

        m.Count.Should().Be(3);
        m.Median.Should().Be(50.0);
    }

    [Fact]
    public void Clear_ResetsToInitialState()
    {
        var m = new MovingMedian<double>(5);
        m.Add(10.0);
        m.Add(20.0);
        m.Add(30.0);

        m.Clear();

        m.Count.Should().Be(0);
        m.Median.Should().Be(0.0);
    }

    [Fact]
    public void OperatorPlus_AddsValue()
    {
        var m = new MovingMedian<double>(3);
        m += 10.0;
        m += 20.0;
        m += 30.0;

        m.Median.Should().Be(20.0);
    }

    [Fact]
    public void ImplicitConversion_ReturnsMedian()
    {
        var m = new MovingMedian<double>(3);
        m.Add(10.0);
        m.Add(20.0);
        m.Add(30.0);

        double result = m;
        result.Should().Be(20.0);
    }

    [Fact]
    public void WorksWithFloat()
    {
        var m = new MovingMedian<float>(3);
        m.Add(10.5f);
        m.Add(20.5f);
        m.Add(30.5f);

        m.Median.Should().BeApproximately(20.5f, 0.001f);
    }

    [Fact]
    public void WorksWithDecimal()
    {
        var m = new MovingMedian<decimal>(3);
        m.Add(10.5m);
        m.Add(20.5m);
        m.Add(30.5m);

        m.Median.Should().Be(20.5m);
    }

    [Fact]
    public void WorksWithInt()
    {
        var m = new MovingMedian<int>(3);
        m.Add(1);
        m.Add(2);
        m.Add(3);

        m.Median.Should().Be(2);
    }

    [Fact]
    public void EvenCount_WithInt_TruncatesTowardZero()
    {
        // int median of [1, 2] = (1+2)/2 = 1 (integer division)
        var m = new MovingMedian<int>(2);
        m.Add(1);
        m.Add(2);

        m.Median.Should().Be(1);
    }

    [Fact]
    public void MedianResistsOutliers()
    {
        var m = new MovingMedian<double>(5);
        m.Add(10.0);
        m.Add(11.0);
        m.Add(12.0);
        m.Add(11.0);
        m.Add(1000.0); // outlier

        // sorted: [10, 11, 11, 12, 1000] → median = 11
        m.Median.Should().Be(11.0);
    }

    [Fact]
    public void ToString_ReturnsMedianAsString()
    {
        var m = new MovingMedian<double>(3);
        m.Add(10.0);
        m.Add(20.0);
        m.Add(30.0);

        m.ToString().Should().Be("20");
    }
}
