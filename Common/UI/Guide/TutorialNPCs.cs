using System.Collections.Generic;

namespace PathOfTerraria.Common.UI.Guide;

/// <summary>
/// Handles NPC spawns while the tutorial is active.
/// </summary>
internal class TutorialNPCs : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		Player p = spawnInfo.Player;

		//Is the player is in a surface evil biome and has not completed the tutorial, disable spawns
		if (ModContent.GetInstance<TutorialSystem>().FreeDay && (p.ZoneCrimson || p.ZoneCorrupt) && (p.Center.Y / 16) <= Main.worldSurface)
		{
			pool.Clear();
		}
	}
}