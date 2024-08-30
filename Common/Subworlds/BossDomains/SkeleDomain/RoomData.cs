using Microsoft.CodeAnalysis.CSharp.Syntax;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

public readonly struct SpikeballInfo(Point16 position, float length, bool? spinClockwise = null, float? spinSpeed = null)
{
	public readonly Point16 Position = position;
	public readonly float Length = length;
	public readonly bool? SpinClockwise = spinClockwise;
	public readonly float SpinSpeed = spinSpeed ?? 0.06f;
}

public class EngageTimerInfo(Point16 position, int ticks)
{
	public readonly Point16 Position = position;

	public int Ticks = ticks;
}

public readonly struct RoomData(WireColor color, OpeningType opening, Point openingLoc, Point wireLoc, List<SpikeballInfo> spikeBalls = null, List<EngageTimerInfo> timers = null)
{
	public readonly WireColor Wire = color;
	public readonly OpeningType Opening = opening;
	public readonly Point OpeningLocation = openingLoc;
	public readonly Point WireConnection = wireLoc;
	public readonly List<SpikeballInfo> Spikeballs = spikeBalls ?? [];
	public readonly List<EngageTimerInfo> Timers = timers ?? [];

	public void PlaceRoom(int x, int y, int id, Vector2 origin)
	{
		Point16 position = StructureTools.PlaceByOrigin("Assets/Structures/SkeletronDomain/Room_" + id, new Point16(x, y), origin);

		foreach (SpikeballInfo info in Spikeballs)
		{
			IEntitySource source = Entity.GetSource_NaturalSpawn();
			int type = ModContent.NPCType<ControllableSpikeball>();
			int whoAmI = NPC.NewNPC(source, (x + info.Position.X) * 16 + 6, (y + info.Position.Y) * 16 - 8, type, 0, info.SpinSpeed, 0, 0, info.Length);
		}

		if (Timers.Count > 0)
		{
			List<EngageTimerInfo> adjustedTimers = [];

			foreach (EngageTimerInfo info in Timers)
			{
				adjustedTimers.Add(new EngageTimerInfo(new Point16(info.Position.X + x, info.Position.Y + y), info.Ticks));
			}

			ModContent.GetInstance<RoomDatabase>().AddTimerInfo(adjustedTimers);
		}
	}
}

public enum WireColor : byte
{
	Red,
	Blue,
	Green,
	Yellow,
}

public enum OpeningType : byte
{
	Left,
	Right,
	Below,
	Above
}