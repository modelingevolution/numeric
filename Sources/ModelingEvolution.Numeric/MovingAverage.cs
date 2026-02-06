using System.Numerics;

namespace ModelingEvolution.Numeric;

/// <summary>
/// High-performance moving average calculator with zero allocations.
/// Uses circular buffer and running sum for O(1) add and average operations.
/// </summary>
/// <typeparam name="T">Floating-point numeric type</typeparam>
public struct MovingAverage<T> where T : INumber<T>
{
    private readonly T[] _buffer;
    private readonly int _capacity;
    private int _index;
    private int _count;
    private T _sum;

    /// <summary>
    /// Creates a moving average calculator with specified window size.
    /// </summary>
    /// <param name="windowSize">Number of values to average (must be > 0)</param>
    public MovingAverage(int windowSize)
    {
        if (windowSize <= 0)
            throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        _capacity = windowSize;
        _buffer = new T[windowSize];
        _index = 0;
        _count = 0;
        _sum = T.Zero;
    }

    /// <summary>
    /// Current number of values in the buffer (0 to capacity).
    /// </summary>
    public readonly int Count => _count;

    /// <summary>
    /// Maximum number of values that can be stored.
    /// </summary>
    public readonly int Capacity => _capacity;

    /// <summary>
    /// Current average of all values in the buffer.
    /// Returns zero if no values have been added.
    /// </summary>
    public readonly T Average
    {
        get
        {
            if (_count == 0)
                return T.Zero;

            return _sum / T.CreateChecked(_count);
        }
    }

    /// <summary>
    /// Adds a new value to the moving average.
    /// If buffer is full, replaces the oldest value.
    /// </summary>
    /// <param name="value">Value to add</param>
    public void Add(T value)
    {
        // Subtract old value if buffer is full
        if (_count == _capacity)
        {
            _sum -= _buffer[_index];
        }
        else
        {
            _count++;
        }

        // Add new value
        _buffer[_index] = value;
        _sum += value;

        // Move to next position (circular)
        _index = (_index + 1) % _capacity;
    }

    /// <summary>
    /// Resets the moving average to empty state.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_buffer);
        _index = 0;
        _count = 0;
        _sum = T.Zero;
    }

    /// <summary>
    /// Adds a value using += operator.
    /// </summary>
    public static MovingAverage<T> operator +(MovingAverage<T> avg, T value)
    {
        avg.Add(value);
        return avg;
    }

    /// <summary>
    /// Implicit conversion to T returns the current average.
    /// </summary>
    public static implicit operator T(MovingAverage<T> avg) => avg.Average;

    /// <summary>
    /// Returns the current average as a string.
    /// </summary>
    public override readonly string ToString() => Average.ToString() ?? string.Empty;
}
