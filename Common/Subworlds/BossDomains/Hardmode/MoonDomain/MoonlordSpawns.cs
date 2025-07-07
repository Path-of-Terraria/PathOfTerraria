using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
using PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;
using PathOfTerraria.Content.NPCs.BossDomain.PlantDomain;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

[ReinitializeDuringResizeArrays]
internal class MoonlordSpawns : GlobalNPC
{
	public static bool[] AllUnsafeArray = WallID.Sets.Factory.CreateBoolSet(false);
	public static bool[] OldWallHouse = null;

	public override void Load()
	{
		On_NPC.NewNPC += FixCrawltipedes;
		On_NPC.SpawnNPC += HijackWallSafety;
	}

	private static void HijackWallSafety(On_NPC.orig_SpawnNPC orig)
	{
		if (SubworldSystem.Current is MoonLordDomain)
		{
			OldWallHouse = Main.wallHouse;
			Main.wallHouse = AllUnsafeArray;
		}

		orig();

		if (SubworldSystem.Current is MoonLordDomain)
		{
			Main.wallHouse = OldWallHouse;
		}
	}

	private static int FixCrawltipedes(On_NPC.orig_NewNPC orig, IEntitySource src, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int t)
	{
		if (Start == 0 && Type is NPCID.SolarCrawltipedeHead or NPCID.StardustWormHead or NPCID.CultistDragonHead or NPCID.WyvernHead or NPCID.SeekerHead 
			or NPCID.DuneSplicerHead)
		{
			Start = 1; // The fix is so easy yet vanilla can't for some reason >:(
		}

		return orig(src, X, Y, Type, Start, ai0, ai1, ai2, ai3, t);
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		if (MoonDomainSystem.BossSpawned)
		{
			pool.Clear();
			return;
		}

		pool[0] = 0;

		if (spawnInfo.SpawnTileY < MoonLordDomain.PlanetTop) // Entering Moon Lord area
		{
			pool[NPCID.BlueSlime] = 1f;
		}
		else if (spawnInfo.SpawnTileY < MoonLordDomain.CloudTop) // Lunar planets area
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
		else if (spawnInfo.SpawnTileY < MoonLordDomain.CloudBottom) // Cloud area
		{
			pool[NPCID.WyvernHead] = 0.2f;
			pool[NPCID.CultistDragonHead] = 0.2f;
			pool[NPCID.DeadlySphere] = 0.2f;
		}
		else if (spawnInfo.SpawnTileY < MoonLordDomain.TerrariaHeight) // Between cloud and Terraria
		{
			pool[NPCID.WanderingEye] = 0.2f;
			pool[NPCID.MothronSpawn] = 0.2f;
		}
		else if (spawnInfo.SpawnTileY < MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, MoonlordTerrainGen.StoneCutoff)) // Terraria nonsense area
		{
			pool[NPCID.Paladin] = 0.2f;
			pool[ModContent.NPCType<Ent>()] = 0.2f;
			pool[NPCID.GoblinShark] = 0.1f;
			pool[NPCID.Mothron] = 0.2f;
			pool[ModContent.NPCType<Minitera>()] = 0.1f;
		}
		else if (spawnInfo.SpawnTileY < MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, MoonlordTerrainGen.DirtCutoff)) // Terraria ice/underground area
		{
			pool[NPCID.SeekerHead] = 0.2f;
			pool[ModContent.NPCType<GreaterFairy>()] = 0.2f;
			pool[NPCID.DuneSplicerHead] = 0.1f;
			pool[NPCID.GiantTortoise] = 0.1f;
			pool[NPCID.GiantCursedSkull] = 0.1f;
			pool[NPCID.SandElemental] = 0.1f;
		}
		else
		{
			pool[NPCID.ArmoredSkeleton] = 0.3f;
			pool[NPCID.RockGolem] = 0.1f;
			pool[NPCID.PirateGhost] = 0.1f;
			pool[NPCID.BloodSquid] = 0.1f;
		}
	
		pool[NPCID.Shimmerfly] = 0.05f;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not MoonLordDomain || spawnRate == int.MinValue)
		{
			return;
		}

		if (MoonDomainSystem.BossSpawned)
		{
			return;
		}

		spawnRate = (int)(spawnRate * 0.2f);
		maxSpawns += 5;
	}

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		if (SubworldSystem.Current is not MoonLordDomain)
		{
			return;
		}

		npc.lifeMax = (int)(npc.lifeMax * (Main.expertMode ? 0.33f : 0.5f));
	}
}
