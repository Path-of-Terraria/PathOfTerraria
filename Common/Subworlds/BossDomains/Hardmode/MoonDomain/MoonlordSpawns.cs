using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

internal class MoonlordSpawns : GlobalNPC
{
	public override void Load()
	{
		On_NPC.NewNPC += FixCrawltipedes;
	}

	private int FixCrawltipedes(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
	{
		if (Start == 0 && Type is NPCID.SolarCrawltipedeHead or NPCID.StardustWormHead or NPCID.CultistDragonHead or NPCID.WyvernHead)
		{
			Start = 1; // The fix is so easy yet vanilla can't for some reason.
		}

		return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		pool[0] = 0;

		if (spawnInfo.SpawnTileY < MoonLordDomain.CloudTop)
		{
			if (MoonLordTileCounts.SolarTiles)
			{
				pool[NPCID.SolarCrawltipedeHead] = 0.3f;
				pool[NPCID.SolarCorite] = 0.7f;
			}

			if (MoonLordTileCounts.NebulaTiles)
			{
				pool[NPCID.NebulaBrain] = 0.3f;
				pool[NPCID.NebulaHeadcrab] = 0.7f;
			}

			if (MoonLordTileCounts.StardustTiles)
			{
				pool[NPCID.StardustCellBig] = 0.3f;
				pool[NPCID.StardustJellyfishBig] = 0.3f;
				pool[NPCID.StardustWormHead] = 0.7f;
			}

			if (MoonLordTileCounts.VortexTiles)
			{
				pool[NPCID.VortexHornet] = 0.3f;
				pool[NPCID.VortexRifleman] = 0.3f;
			}
		}
		else if (spawnInfo.SpawnTileY < MoonLordDomain.CloudBottom)
		{
			pool[NPCID.WyvernHead] = 0.2f;
			pool[NPCID.CultistDragonHead] = 0.2f;
			pool[NPCID.DeadlySphere] = 0.2f;
			pool[NPCID.AncientCultistSquidhead] = 0.2f;
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not MoonLordDomain || spawnRate == int.MinValue)
		{
			return;
		}

		spawnRate = (int)(spawnRate * 0.05f);
		maxSpawns += 5;
	}

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		//if (npc.type is NPCID.SolarCrawltipedeHead or NPCID.SolarCorite or NPCID.NebulaBrain or NPCID.NebulaHeadcrab or NPCID.StardustCellBig or NPCID.StardustJellyfishBig
		//	or NPCID.StardustWormHead or NPCID.VortexHornet or NPCID.VortexRifleman)
		//{
		npc.lifeMax = (int)(npc.lifeMax * (Main.expertMode ? 0.33f : 0.5f));
		//}
	}
}
