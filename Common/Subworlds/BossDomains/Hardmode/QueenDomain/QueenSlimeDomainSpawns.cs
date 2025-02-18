using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class QueenSlimeDomainSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not QueenSlimeDomain)
		{
			return;
		}

		pool[0] = 0;

		// Underground spawns first, otherwise aboveground
		if (QueenSlimeDomain.ModDistance(spawnInfo.Player.Center, QueenSlimeScene.GetCircleCenter()) > 696 * 16)
		{
			pool[NPCID.ChaosElemental] = 0.3f;
			pool[NPCID.Gastropod] = 0.8f;
			pool[NPCID.IlluminantBat] = 0.8f;
			pool[NPCID.IlluminantSlime] = 1;
			pool[NPCID.EnchantedSword] = 0.2f;
		}
		else
		{
			pool[NPCID.Pixie] = 1f;
			pool[NPCID.Unicorn] = 0.1f;
			pool[NPCID.LightMummy] = 0.3f;
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		spawnRate = (int)(spawnRate * 0.8f);
		maxSpawns += 2;
	}
}
