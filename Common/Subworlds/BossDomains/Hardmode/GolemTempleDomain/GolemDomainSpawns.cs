using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.GolemTempleDomain;

internal class GolemDomainSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not GolemDomain)
		{
			return;
		}

		pool[0] = 0;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not GolemDomain || spawnRate == int.MinValue)
		{
			return;
		}

		maxSpawns = 0;
	}
}
