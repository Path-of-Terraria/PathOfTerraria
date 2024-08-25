namespace PathOfTerraria.Common.Waypoints;

public sealed class HomeWaypoint : ModWaypoint
{
	public override string IconPath => base.IconPath.Replace("Common", "Assets");

	public override string PreviewPath => base.PreviewPath.Replace("Common", "Assets");

	public override void Teleport(Player player) { }
}