using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.NPCs.BossDomain.SkeletronDomain;
using StructureHelper;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

/// <summary>
/// Stores all the info needed for a spikeball. <paramref name="spinClockwise"/> defaults to random, 
/// <paramref name="spinSpeed"/> defaults to 0.06f.
/// </summary>
/// <param name="position">Position of the spikeball. This is offset by 2 tiles up.</param>
/// <param name="length">Length of the spikeball's chain.</param>
/// <param name="spinClockwise">Which way the spikeball rotates. Defaults to random.</param>
/// <param name="spinSpeed">How quickly the spikeball spins. Defaults to 0.06f.</param>
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
		Point16 position = StructureTools.PlaceByOrigin("Assets/Structures/SkeletronDomain/Room_" + id, new Point16(x, y), origin, null, false);
		AddSpawns(x, y);
	}

	public Rectangle PlaceRoom(int x, int y, int id, Point origin)
	{
		x -= origin.X;
		y -= origin.Y;

		string structure = "Assets/Structures/SkeletronDomain/Room_" + id;
		Point16 position = StructureTools.PlaceByOrigin(structure, new Point16(x, y), Vector2.Zero, null, false);
		var size = new Point16();
		Generator.GetDimensions(structure, PoTMod.Instance, ref size);
		AddSpawns(x, y);

		return new Rectangle(x, y, size.X, size.Y);
	}

	private void AddSpawns(int x, int y)
	{
		foreach (SpikeballInfo info in Spikeballs)
		{
			IEntitySource source = Entity.GetSource_NaturalSpawn();
			int type = ModContent.NPCType<ControllableSpikeball>();
			int dir = info.SpinClockwise is null ? Main.rand.NextBool(2) ? -1 : 1 : (info.SpinClockwise.Value ? -1 : 1);
			int whoAmI = NPC.NewNPC(source, (x + info.Position.X) * 16 + 6, (y + info.Position.Y) * 16 - 8, type, 1, info.SpinSpeed * dir, 0, 0, info.Length);

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, whoAmI);
			}
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