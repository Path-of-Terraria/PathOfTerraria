using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.DukeDomain;

internal class FishronDomainSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not FishronDomain || NPC.AnyNPCs(NPCID.DukeFishron))
		{
			return;
		}

		pool[0] = 0;

		pool[NPCID.MushiLadybug] = 0.6f;
		pool[NPCID.AnomuraFungus] = 0.6f;
		pool[NPCID.GiantFungiBulb] = 0.3f;
		pool[NPCID.FungiBulb] = 0.1f;

		if (spawnInfo.Water)
		{
			pool[NPCID.FungoFish] = 0.4f;
		}
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (SubworldSystem.Current is not FishronDomain)
		{
			return;
		}

		if (npc.type == NPCID.GiantFungiBulb)
		{
			int i = 0;
			npc.ai[0] = (int)npc.Center.X / 16;
			npc.ai[1] = (int)npc.Center.Y / 16 + 2;
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not FishronDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate = (int)(spawnRate * 0.8f);
		maxSpawns += 2;
	}
}
