using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.EoLDomain;

internal class EoLDomainSpawns : GlobalNPC
{
	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not EmpressDomain)
		{
			return;
		}

		spawnRate = int.MaxValue / 4;
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not EmpressDomain)
		{
			return;
		}

		pool.Clear();
	}
}
