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
		bool hasNotCompletedTutorial = !Main.LocalPlayer.GetModPlayer<TutorialPlayer>().CompletedTutorial;
		
		return SubworldSystem.Current is not null && !hasNotCompletedTutorial;
	}
}