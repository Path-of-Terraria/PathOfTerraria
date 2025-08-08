using PathOfTerraria.Common.Systems.Networking.Handlers;
using SubworldLibrary;
using System.Collections.Generic;

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
		if (SubworldSystem.Current is null && CachedBossesDowned.Count > 0)
		{
			foreach (int id in CachedBossesDowned)
			{
				ModContent.GetInstance<SyncBossDownedHandler>().Send(id);
			}

			CachedBossesDowned.Clear();
		}
	}
}
