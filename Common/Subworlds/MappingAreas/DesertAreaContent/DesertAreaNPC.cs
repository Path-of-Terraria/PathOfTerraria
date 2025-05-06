using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Content.NPCs.Mapping.Desert;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.DesertAreaContent;

internal class DesertAreaNPC : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not DesertArea)
		{
			return;
		}

		pool.Clear();

		if (DesertArea.ActiveDevourer())
		{
			return;
		}

		pool[ModContent.NPCType<HauntedHead>()] = 0.5f;
		pool[ModContent.NPCType<ScarabSwarmController>()] = 0.3f;
		pool[NPCID.Mummy] = 1f;
		pool[NPCID.SandShark] = 0.1f;
		pool[NPCID.DesertGhoul] = 0.9f;
		pool[NPCID.DesertDjinn] = 0.3f;
		pool[NPCID.Scorpion] = 0.1f;
		pool[NPCID.ScorpionBlack] = 0.1f;
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is not QueenSlimeDomain || spawnRate == int.MinValue)
		{
			return;
		}

		maxSpawns -= 3;
		spawnRate = (int)(spawnRate * 2f);
	}

	public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
		if (npc.type == NPCID.DesertDjinn && SubworldSystem.Current is DesertArea)
		{
			npc.life /= 4;
		}
	}
}
