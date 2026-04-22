using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Swamp.NPCs;
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

			if (spawnInfo.Water)
			{
				pool.Add(ModContent.NPCType<SwampCroc>(), 0.2f);
				pool.Add(ModContent.NPCType<Mudsquit>(), 0.8f);
			}
			else
			{
				pool.Add(ModContent.NPCType<DragonFly>(), 1f);
			}
			// No natural spawning, enemies are either generated or come from encounters.
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		spawnRate = (int)(spawnRate * 0.5f);
		maxSpawns += 4;

		if (player.HasBuff<SwampAlgaeBuff>())
		{
			maxSpawns += 8;
			spawnRate = (int)(spawnRate * 0.7f);
		}
	}
}
