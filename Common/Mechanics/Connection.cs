namespace PathOfTerraria.Common.Mechanics;

[Flags]
internal enum EdgeFlags : byte
{
	None,
	Hidden = 1 << 0,
}

internal readonly record struct Edge(Allocatable Start, Allocatable End, EdgeFlags Flags)
{
	public readonly Allocatable Start = Start;
	public readonly Allocatable End = End;
	public readonly EdgeFlags Flags = Flags;

	public bool Contains(Allocatable p)
	{
		return p == Start || p == End;
	}

	public Allocatable Other(Allocatable p)
	{
		return p == Start ? End : Start;
	}
}