using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class EditSkeleDomainSpawnPool : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (SubworldSystem.Current is SkeletronDomain)
		{
			pool.Clear();

			if (spawnInfo.Player.Center.Y / 16 > 180)
			{
				pool.Add(NPCID.Rat, 0.05f);
				pool.Add(NPCID.CursedSkull, 0.025f);
				pool.Add(NPCID.AngryBones, 0.1f);
				pool.Add(NPCID.BlazingWheel, 0.01f);
				pool.Add(NPCID.DungeonSlime, 0.001f);
			}
			else
			{
				pool.Add(NPCID.Zombie, 0.5f);
				pool.Add(NPCID.DemonEye, 0.5f);
				pool.Add(NPCID.Skeleton, 0.5f);
			}
		}
	}

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		if (SubworldSystem.Current is SkeletronDomain)
		{
			spawnRate *= 5;
			maxSpawns /= 5;
		}
	}
}
