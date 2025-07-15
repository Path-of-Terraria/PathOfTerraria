using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.KingDomain;

internal class KingSlimeDomainSpawns :  GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not KingSlimeDomain)
		{
			return;
		}

		pool[0] = 0;
		pool[NPCID.Pinky] = 0.005f;
		pool[NPCID.YellowSlime] = 0.1f;
		pool[NPCID.PurpleSlime] = 0.2f;
		pool[NPCID.RedSlime] = 0.3f;
		pool[NPCID.GreenSlime] = 0.6f;
		pool[NPCID.BlueSlime] = 1.2f;
	}
}
