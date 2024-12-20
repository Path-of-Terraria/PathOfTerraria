using SubworldLibrary;

namespace PathOfTerraria.Common.Waypoints;

public sealed class HomeWaypoint : ModWaypoint
{
	public override string LocationEnum => "Overworld";

	public override void Teleport(Player player)
	{
		SubworldSystem.Exit();
	}
}