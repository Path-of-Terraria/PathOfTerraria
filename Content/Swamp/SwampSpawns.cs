using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Swamp;

internal class SwampSpawns : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			pool.Clear();

			// No natural spawning, enemies are either generated or come from encounters.
		}
	}
}
