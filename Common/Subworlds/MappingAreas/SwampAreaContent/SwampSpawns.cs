using PathOfTerraria.Content.NPCs.Mapping.Swamp;
using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

internal class SwampSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			pool.Clear();

			if (spawnInfo.Water)
			{
				pool[ModContent.NPCType<SwampCroc>()] = 0.3f;
			}
		}
	}
}
