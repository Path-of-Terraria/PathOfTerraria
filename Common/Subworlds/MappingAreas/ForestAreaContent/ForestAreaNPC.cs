using PathOfTerraria.Content.NPCs.Mapping.Forest;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.ForestAreaContent;

internal class ForestAreaNPC : GlobalNPC
{
	public override void Load()
	{
		On_NPC.AI += HijackNightForEnemiesInForest;
	}

	private void HijackNightForEnemiesInForest(On_NPC.orig_AI orig, NPC self)
	{
		if (SubworldSystem.Current is ForestArea && self.type is NPCID.Wraith or NPCID.Raven or NPCID.PossessedArmor)
		{
			Main.dayTime = false;
			orig(self);
			Main.dayTime = true;
		}
		else
		{
			orig(self);
		}
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is not ForestArea)
		{
			return;
		}

		pool.Clear();

		pool[ModContent.NPCType<Entling>()] = 1;
		pool[ModContent.NPCType<EntlingAlt>()] = 1;
		pool[ModContent.NPCType<ClumsyEntling>()] = 1;
		pool[ModContent.NPCType<Ent>()] = 0.2f;
		pool[NPCID.Wraith] = 0.9f;
		pool[NPCID.Raven] = 0.4f;
		pool[NPCID.PossessedArmor] = 0.6f;
	}
}
