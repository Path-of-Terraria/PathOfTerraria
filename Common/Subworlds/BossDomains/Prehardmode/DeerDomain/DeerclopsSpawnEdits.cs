using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

internal class DeerclopsSpawnEdits : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not DeerclopsDomain)
		{
			return;
		}

		pool.Clear();

		if (spawnInfo.SpawnTileY > Main.worldSurface)
		{
			pool.Add(NPCID.IceBat, 0.65f);
			pool.Add(NPCID.IceSlime, 0.65f);
			pool.Add(NPCID.SpikedIceSlime, 0.15f);
			pool.Add(NPCID.UndeadViking, 0.35f);
			pool.Add(NPCID.ArmoredViking, 0.01f);
			pool.Add(NPCID.SnowFlinx, 0.25f);
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is DeerclopsDomain)
		{
			spawnRate = 80;
			maxSpawns = 4;
		}
	}
}