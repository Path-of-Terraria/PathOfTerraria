using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.EoCDomain;

internal class EoCDomainSpawns :  GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not EyeDomain)
		{
			return;
		}

		pool[0] = 0;
		pool[NPCID.PurpleSlime] = 0.2f;
		pool[NPCID.Zombie] = 0.3f;
		pool[NPCID.DemonEye] = 0.6f;
	}
}
