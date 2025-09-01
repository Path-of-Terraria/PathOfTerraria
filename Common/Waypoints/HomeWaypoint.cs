using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.UI.Guide;
using SubworldLibrary;

namespace PathOfTerraria.Common.Waypoints;

public sealed class HomeWaypoint : ModWaypoint
{
	public override string LocationEnum => "Overworld";

	public override void Teleport(Player player)
	{
		SubworldSystem.Exit();
	}

	public override bool CanGoto()
	{
		bool isHomeObeliskMissing = !ModContent.GetInstance<PersistentDataSystem>().ObelisksByLocation.Contains("Overworld");
		
		return SubworldSystem.Current is not null && !isHomeObeliskMissing;
	}
}