using System.Numerics;

namespace PathOfTerraria.Utilities;

internal struct Counter<T> where T : INumber<T>
{
	public T Value;

	public void Set(T value)
	{
		Value = value;
	}
	public void SetIfGreater(T value)
	{
		if (value > Value) { Set(value); }
	}

	public void CountDown()
	{
		if (Value > T.Zero) { Value--; }
	}
}
