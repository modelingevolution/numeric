using System.Numerics;

namespace ModelingEvolution.Numeric;

/// <summary>
/// Moving median calculator over a sliding window.
/// Maintains a sorted array alongside a circular buffer for O(1) median and O(n) add.
/// </summary>
/// <typeparam name="T">Numeric type</typeparam>
public struct MovingMedian<T> where T : INumber<T>
{
    private readonly T[] _buffer;
    private readonly T[] _sorted;
    private readonly int _capacity;
    private int _index;
    private int _count;
    private static readonly T _two = T.CreateChecked(2);

    /// <summary>
    /// Creates a moving median calculator with specified window size.
    /// </summary>
    /// <param name="windowSize">Number of values in the window (must be > 0)</param>
    public MovingMedian(int windowSize)
    {
        if (windowSize <= 0)
            throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        _capacity = windowSize;
        _buffer = new T[windowSize];
        _sorted = new T[windowSize];
        _index = 0;
        _count = 0;
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
    /// Current median of all values in the buffer. O(1).
    /// For even count, returns the average of the two middle values.
    /// Returns zero if no values have been added.
    /// </summary>
    public readonly T Median
    {
        get
        {
            if (_count == 0)
                return T.Zero;

            int mid = _count / 2;
            if (_count % 2 == 1)
                return _sorted[mid];

            return (_sorted[mid - 1] + _sorted[mid]) / _two;
        }
    }

    /// <summary>
    /// Adds a new value to the moving median.
    /// If buffer is full, replaces the oldest value.
    /// O(n) due to sorted array maintenance, O(1) median read.
    /// </summary>
    /// <param name="value">Value to add</param>
    public void Add(T value)
    {
        if (_count == _capacity)
        {
            var old = _buffer[_index];
            RemoveFromSorted(old, _count);
        }
        else
        {
            _count++;
        }

        InsertIntoSorted(value, _count - 1);

        _buffer[_index] = value;
        _index = (_index + 1) % _capacity;
    }

    /// <summary>
    /// Inserts value into _sorted which currently has 'length' elements.
    /// After call, _sorted has length+1 valid elements.
    /// </summary>
    private void InsertIntoSorted(T value, int length)
    {
        int pos = LowerBound(_sorted, value, length);
        Array.Copy(_sorted, pos, _sorted, pos + 1, length - pos);
        _sorted[pos] = value;
    }

    /// <summary>
    /// Removes one occurrence of value from _sorted which currently has 'length' elements.
    /// After call, _sorted has length-1 valid elements.
    /// </summary>
    private void RemoveFromSorted(T value, int length)
    {
        int pos = LowerBound(_sorted, value, length);
        Array.Copy(_sorted, pos + 1, _sorted, pos, length - pos - 1);
    }

    /// <summary>
    /// Returns the index of the first element >= value in sorted[0..length).
    /// </summary>
    private static int LowerBound(T[] sorted, T value, int length)
    {
        int lo = 0, hi = length;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (sorted[mid] < value)
                lo = mid + 1;
            else
                hi = mid;
        }
        return lo;
    }

    /// <summary>
    /// Resets the moving median to empty state.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_buffer);
        Array.Clear(_sorted);
        _index = 0;
        _count = 0;
    }

    /// <summary>
    /// Adds a value using += operator.
    /// </summary>
    public static MovingMedian<T> operator +(MovingMedian<T> m, T value)
    {
        m.Add(value);
        return m;
    }

    /// <summary>
    /// Implicit conversion to T returns the current median.
    /// </summary>
    public static implicit operator T(MovingMedian<T> m) => m.Median;

    /// <summary>
    /// Returns the current median as a string.
    /// </summary>
    public override readonly string ToString() => Median.ToString() ?? string.Empty;
}
