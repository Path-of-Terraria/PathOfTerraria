using System.Collections.Generic;

namespace PathOfTerraria.Common.UI.Guide;

/// <summary>
/// Handles NPC spawns while the tutorial is active.
/// </summary>
internal class TutorialNPCs : GlobalNPC
{
	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		Player plr = spawnInfo.Player;

		// If the player is in a surface evil on the "free day", disable spawns
		if (ModContent.GetInstance<TutorialSystem>().FreeDay && (plr.ZoneCrimson || plr.ZoneCorrupt) && (plr.Center.Y / 16) <= Main.worldSurface)
		{
			pool.Clear();
		}
	}
}