using PathOfTerraria.Content.NPCs.BossDomain.BrainDomain;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.BoCDomain;

internal class BoCDomainSpawns :  GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not BrainDomain)
		{
			return;
		}

		pool[0] = 0;
		pool[NPCID.Crimera] = 0.1f;
		pool[ModContent.NPCType<Minera>()] = 0.3f;
		pool[NPCID.FaceMonster] = 0.4f;
		pool[NPCID.BloodCrawler] = 0.4f;
	}
}
