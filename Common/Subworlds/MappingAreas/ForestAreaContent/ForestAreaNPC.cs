using PathOfTerraria.Content.NPCs.Mapping.Forest;
using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.ForestAreaContent;

internal class ForestAreaNPC : GlobalNPC
{
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
	}
}
