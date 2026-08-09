using System.Numerics;

#pragma warning disable IDE0032 // Use auto property

namespace PathOfTerraria.Utilities;

/// <summary> A number that conveniently caches its squared variant. </summary>
public struct SqrScalar<T> where T : INumber<T>
{
    private T val;
    private T sqrVal;

	public T Value
    {
        readonly get => val;
        set => (val, sqrVal) = (value, value * value);
    }
    public readonly T SqrValue => sqrVal;

	public SqrScalar(T min)
    {
        Value = min;
    }
}

/// <summary> A range that conveniently caches squared values. </summary>
public struct SqrRange<T> where T : INumber<T>
{
    private SqrScalar<T> min;
    private SqrScalar<T> max;

	public T Min
    {
        readonly get => min.Value;
        set => min.Value = value;
    }
	public T Max
    {
        readonly get => max.Value;
        set => max.Value = value;
    }
    public readonly T SqrMin => min.SqrValue;
    public readonly T SqrMax => max.SqrValue;

	public SqrRange(T min, T max)
    {
        Min = min;
        Max = max;
    }
}
