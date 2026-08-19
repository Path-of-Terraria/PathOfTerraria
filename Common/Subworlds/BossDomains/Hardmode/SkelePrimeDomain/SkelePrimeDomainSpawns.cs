using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;
using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.SkelePrimeDomain;

internal class SkelePrimeDomainSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not PrimeDomain)
		{
			return;
		}

		pool[0] = 0;

		pool[ModContent.NPCType<SecurityDrone>()] = 0.2f;
		pool[ModContent.NPCType<CircuitSkull>()] = 0.2f;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not PrimeDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate *= 20;
		maxSpawns /= 2;
	}
}
