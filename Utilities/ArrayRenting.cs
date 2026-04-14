using System.Buffers;

namespace PathOfTerraria.Utilities;

internal readonly record struct RentedArray<T> : IDisposable
{
    public T[] Array { get; }
    public int Length { get; }
    public Span<T> Span => Array.AsSpan(0, Length);
    public ref T this[int index] => ref Array[index];

    public RentedArray(int minLength)
    {
        Array = ArrayPool<T>.Shared.Rent(minLength);
        Length = minLength;
    }

	public void Dispose()
	{
		ArrayPool<T>.Shared.Return(Array);
	}

    public static implicit operator T[](RentedArray<T> rent)
    {
        return rent.Array;
    }
}
