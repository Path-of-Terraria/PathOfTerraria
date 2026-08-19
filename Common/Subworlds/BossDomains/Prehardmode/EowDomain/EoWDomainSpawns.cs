using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.EowDomain;

internal class EoWDomainSpawns :  GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not EaterDomain)
		{
			return;
		}

		pool[0] = 0;
		pool[NPCID.EaterofSouls] = 0.5f;
		pool[NPCID.DevourerHead] = 0.05f;
	}
}
