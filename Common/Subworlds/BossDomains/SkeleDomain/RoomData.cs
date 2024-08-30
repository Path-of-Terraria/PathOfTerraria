using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

public readonly struct SpikeballInfo(Point16 position, float length, bool? spinClockwise)
{
	public readonly Point16 Position = position;
	public readonly float Length = length;
	public readonly bool? SpinClockwise = spinClockwise;
}

public readonly struct RoomData(WireColor color, OpeningType opening, Point openingLoc, Point wireLoc, List<SpikeballInfo> spikeBalls)
{
	public readonly WireColor Wire = color;
	public readonly OpeningType Opening = opening;
	public readonly Point OpeningLocation = openingLoc;
	public readonly Point WireConnection = wireLoc;
	public readonly List<SpikeballInfo> Spikeballs = spikeBalls;

	public void PlaceRoom(int x, int y, int id, Vector2 origin)
	{
		Point16 position = StructureTools.PlaceByOrigin("Assets/Structures/SkeletronDomain/Room_" + id, new Point16(x, y), origin);

		foreach (SpikeballInfo info in Spikeballs)
		{
			int whoAmI = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (x + info.Position.X) * 16 + 6, (y + info.Position.Y) * 16 - 8, NPCID.SpikeBall);
			NPC npc = Main.npc[whoAmI];
			SpikeballNPC ball = npc.GetGlobalNPC<SpikeballNPC>();
			npc.ai[1] = npc.Center.X;
			npc.ai[2] = npc.Center.Y;
			ball.Length = info.Length;

			if (info.SpinClockwise is not null)
			{
				ball.Direction = info.SpinClockwise.Value ? 1 : -1;
			}
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