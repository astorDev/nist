using System.Collections;

namespace Nist;

public class Fibonacci : IEnumerable<int>
{
    public static Fibonacci Sequence { get; } = new Fibonacci();
    public static int At(int position) => Sequence.ElementAt(position);

    public IEnumerator<int> GetEnumerator()
    {
        return new FibonacciEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class FibonacciEnumerator : IEnumerator<int>
{
    public int Previous { get; set; } = 1;
    public int Current { get; set; } = 0;

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool MoveNext()
    {
        var toBecomePrevious = Current;
        Current += Previous;
        Previous = toBecomePrevious;
        return true;   
    }

    public void Reset()
    {
        Previous = 1;
        Current = 0;
    }
}