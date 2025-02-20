using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.Tools;

public class EngageTimerInfo(Point16 position, int ticks)
{
	public readonly Point16 Position = position;

	public int Ticks = ticks;
}
