using PathOfTerraria.Common.Systems.WorldNavigation;

namespace PathOfTerraria.Common.Waypoints;

public sealed class RavencrestWaypoint : ModWaypoint
{
	public override string IconPath => base.IconPath.Replace("Common", "Assets");

	public override string PreviewPath => base.PreviewPath.Replace("Common", "Assets");

	public override void Teleport(Player player)
	{
	}
}