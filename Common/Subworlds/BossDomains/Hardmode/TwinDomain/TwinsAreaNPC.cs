using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.TwinDomain;

internal class TwinsAreaNPC : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not TwinsDomain)
		{
			return;
		}

		pool.Clear();

		int plrY = (int)(spawnInfo.Player.Center.Y / 16f);

		if (plrY > TwinsDomain.DirtLayerEnd && plrY < TwinsDomain.MetalLayerEnd)
		{
			pool[NPCID.Probe] = 0.2f;
			pool[ModContent.NPCType<Sawblade>()] = 0.3f;
			pool[ModContent.NPCType<CircuitSkull>()] = 0.6f;
			pool[ModContent.NPCType<SecurityDrone>()] = 0.8f;
		}
		else if (plrY >= TwinsDomain.MetalLayerEnd)
		{
			pool[NPCID.Corruptor] = 1;
			pool[NPCID.SeekerHead] = 0.15f;
			pool[NPCID.DesertGhoulCorruption] = 0.8f;
			pool[NPCID.Slimer] = 0.1f;
			pool[NPCID.CursedHammer] = 0.1f;
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not TwinsDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate = (int)MathF.Max(spawnRate * 0.8f, 1f);
		maxSpawns += 4;
	}
}
