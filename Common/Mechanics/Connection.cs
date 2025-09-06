using System.Numerics;

namespace PathOfTerraria.Common.Mechanics;

[Flags]
internal enum EdgeFlags : byte
{
	None,
	Hidden = 1 << 0,
	EffectsOnly = 2 << 0,
}

internal readonly record struct Edge<T>(T Start, T End, EdgeFlags Flags) where T : class
{
	public readonly T Start = Start;
	public readonly T End = End;
	public readonly EdgeFlags Flags = Flags;

	public bool Contains(T p)
	{
		return p == Start || p == End;
	}

	public T Other(T p)
	{
		return p == Start ? End : Start;
	}
}