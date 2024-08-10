// Those waypoints are only "implemented" for UI testing purposes.
namespace PathOfTerraria.Common.Waypoints;

public sealed class DummyWaypointOne : ModWaypoint
{
	public override string IconPath => base.IconPath.Replace("Common", "Assets");

	public override void Teleport(Player player) { }
}

public sealed class DummyWaypointTwo : ModWaypoint
{
	public override string IconPath => base.IconPath.Replace("Common", "Assets");

	public override void Teleport(Player player) { }
}

public sealed class DummyWaypointThree : ModWaypoint
{
	public override string IconPath => base.IconPath.Replace("Common", "Assets");

	public override void Teleport(Player player) { }
}
