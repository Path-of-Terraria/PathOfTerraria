using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.BeeDomain;

internal class QueenBeeDomainSpawns :  GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not QueenBeeDomain)
		{
			return;
		}

		pool[0] = 0;
		pool[NPCID.Hornet] = 0.2f;
		pool[NPCID.JungleSlime] = 0.3f;
		pool[NPCID.SpikedJungleSlime] = 0.05f;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not FishronDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate *= 2;
	}
}
