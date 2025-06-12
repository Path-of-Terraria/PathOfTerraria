using SubworldLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.LunaticDomain;

internal class LunaticStartEventNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		if (npc.type == NPCID.CultistBoss && SubworldSystem.Current is CultistDomain)
		{
			PriorityQueue<Vector2, float> positions = new();
			positions.Enqueue(new Vector2(3200, 4800), Main.rand.NextFloat());
			positions.Enqueue(new Vector2(8800, 5250), Main.rand.NextFloat());
			positions.Enqueue(new Vector2(21600, 5250), Main.rand.NextFloat());
			positions.Enqueue(new Vector2(28000, 4800), Main.rand.NextFloat());

			PriorityQueue<int, float> types = new();
			types.Enqueue(NPCID.LunarTowerNebula, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerSolar, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerStardust, Main.rand.NextFloat());
			types.Enqueue(NPCID.LunarTowerVortex, Main.rand.NextFloat());

			Debug.Assert(positions.Count == types.Count);

			for (int i = 0; i < 4; ++i)
			{
				int type = types.Dequeue();
				Vector2 pos = positions.Dequeue() + Main.rand.NextVector2Circular(300, 300);

				NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, type, 0);
			}
		}
	}
}
