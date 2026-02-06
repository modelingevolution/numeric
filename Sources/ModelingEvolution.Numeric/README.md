# ModelingEvolution.Numeric

Generic numeric utilities for .NET using `INumber<T>` generic math.

## Types

### `MovingAverage<T>`

High-performance moving average calculator with O(1) add and average operations. Uses a circular buffer with running sum - zero allocations after construction.

```csharp
var avg = new MovingAverage<double>(windowSize: 5);
avg.Add(10.0);
avg.Add(20.0);
avg.Add(30.0);

double current = avg; // implicit conversion to T (20.0)

// Operator support
avg += 40.0;
avg += 50.0;

// Window slides automatically
avg += 60.0; // oldest value (10.0) drops out
Console.WriteLine(avg.Average); // (20+30+40+50+60)/5 = 40
```

Works with any `INumber<T>` type: `double`, `float`, `decimal`, `int`, etc.

## Install

```bash
dotnet add package ModelingEvolution.Numeric
```
