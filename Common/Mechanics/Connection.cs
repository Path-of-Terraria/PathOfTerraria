namespace PathOfTerraria.Common.Mechanics;

internal readonly record struct Edge(Allocatable Start, Allocatable End)
{
	public readonly Allocatable Start = Start;
	public readonly Allocatable End = End;

	public bool Contains(Allocatable p)
	{
		return p == Start || p == End;
	}

	public Allocatable Other(Allocatable p)
	{
		return p == Start ? End : Start;
	}
}