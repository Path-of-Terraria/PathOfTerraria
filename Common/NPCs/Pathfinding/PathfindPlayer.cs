using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Pathfinding;

internal class PathfindPlayer : ModPlayer
{
	readonly Pathfinder pathfinder = new();

	int pathfindTimer = 0;

	public override void UpdateEquips()
	{
		pathfindTimer--;

		if (!pathfinder.HasPath || pathfindTimer <= 0)
		{
			pathfinder.DrawPath(Player.Center.ToTileCoordinates16(), Main.MouseWorld.ToTileCoordinates16());
			pathfindTimer = 3;
		}

		if (!pathfinder.HasPath || pathfinder.Path is null)
		{
			return;
		}

		foreach (Pathfinder.FoundPoint item in pathfinder.Path)
		{
			Vector2 vel = Pathfinder.ToVector(item.Direction).ToVector2();
			Dust dust = Dust.NewDustPerfect(item.Position.ToWorldCoordinates(), DustID.YellowStarDust, vel);
			dust.noGravity = true;
		}
	}
}
