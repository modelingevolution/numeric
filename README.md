# ModelingEvolution.Numeric

[![NuGet](https://img.shields.io/nuget/v/ModelingEvolution.Numeric.svg)](https://www.nuget.org/packages/ModelingEvolution.Numeric)

Generic numeric utilities for .NET 10.0 using `INumber<T>` generic math.

## Install

```bash
dotnet add package ModelingEvolution.Numeric
```

## Types

### `MovingAverage<T>`

High-performance moving average calculator with O(1) add and average operations. Uses a circular buffer with running sum - zero allocations after construction.

```csharp
var avg = new MovingAverage<double>(windowSize: 5);

// Add values
avg.Add(10.0);
avg.Add(20.0);
avg.Add(30.0);

Console.WriteLine(avg.Average); // 20.0
Console.WriteLine(avg.Count);   // 3
Console.WriteLine(avg.Capacity); // 5

// Operator support
avg += 40.0;
avg += 50.0;

// Window slides - oldest value drops out
avg += 60.0;
Console.WriteLine(avg.Average); // (20+30+40+50+60)/5 = 40

// Implicit conversion to T
double current = avg; // 40.0

// Reset
avg.Clear();
```

Works with any `INumber<T>` type: `double`, `float`, `decimal`, `int`, etc.

#### Real-world: FPS counter

```csharp
var frameAvg = new MovingAverage<double>(60); // last 60 frames

void OnFrame(double deltaTime)
{
    frameAvg += deltaTime;
    double fps = 1.0 / (double)frameAvg;
    Console.WriteLine($"FPS: {fps:F1}");
}
```

## Build

```bash
dotnet build Sources/ModelingEvolution.Numeric.sln
dotnet test Sources/ModelingEvolution.Numeric.Tests/
```

## License

MIT
