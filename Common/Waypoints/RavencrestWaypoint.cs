using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using SubworldLibrary;

namespace PathOfTerraria.Common.Waypoints;

public sealed class RavencrestWaypoint : ModWaypoint
{
	public override void Teleport(Player player)
	{
		player.GetModPlayer<PersistentReturningPlayer>().ReturnPosition = player.Center;
		SubworldSystem.Enter<RavencrestSubworld>();
	}
}