using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.PlantDomain;

internal class PlanteraDomainSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not PlanteraDomain || NPC.AnyNPCs(NPCID.Plantera))
		{
			return;
		}

		pool[0] = 0;

		pool[NPCID.MossHornet] = 0.12f;
		pool[NPCID.BigMossHornet] = 0.12f;
		pool[NPCID.LittleMossHornet] = 0.12f;
		pool[NPCID.GiantMossHornet] = 0.12f;
		pool[NPCID.TinyMossHornet] = 0.12f;

		pool[NPCID.JungleCreeper] = 0.7f;
		pool[NPCID.SpikedJungleSlime] = 0.4f;
		pool[NPCID.AngryTrapper] = 0.4f;
		pool[NPCID.GiantFlyingFox] = 0.2f;
		pool[NPCID.GiantTortoise] = 0.4f;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not QueenSlimeDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate = (int)(spawnRate * 0.8f);
		maxSpawns += 2;
	}
}
