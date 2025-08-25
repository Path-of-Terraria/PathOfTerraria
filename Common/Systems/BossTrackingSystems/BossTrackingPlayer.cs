using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.BossTrackingSystems;

/// <summary>
/// Temporary fix for networking issues regarding <see cref="BossTracker"/>. Caches bosses downed to set them in the main overworld,
/// as ModPlayers persist easily between subworlds.
/// </summary>
internal class BossTrackingPlayer : ModPlayer
{
	public HashSet<int> CachedBossesDowned = [];

	public override void OnEnterWorld()
	{
		if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is null && CachedBossesDowned.Count > 0)
		{
			foreach (int id in CachedBossesDowned)
			{
				ModContent.GetInstance<SyncBossDownedHandler>().Send(id);
			}

			CachedBossesDowned.Clear();
		}
	}
}
