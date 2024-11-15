using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Pathfinding;

internal class PathfinderPlayer : ModPlayer
{
	public static Point16 LastOrb = new();

	private readonly Pathfinder pathfinder = new(20);

	public override void UpdateEquips()
	{
		if (Main.mouseLeft)
		{
			LastOrb = Main.MouseWorld.ToTileCoordinates16();
		}

		pathfinder.CheckDrawPath(Player.Top.ToTileCoordinates16(), LastOrb, new Vector2(Player.width / 16f * 0.8f, Player.height / 16f * 0.8f),
			null, new(-Player.width / 2.5f, 0));

		if (pathfinder.HasPath && pathfinder.Path.Count > 0)
		{
			foreach (Pathfinder.FoundPoint item in pathfinder.Path)
			{
				var vel = Pathfinder.ToVector2(item.Direction);
				int id = DustID.Poisoned;
				var dust = Dust.NewDustPerfect(item.Position.ToWorldCoordinates(), id, vel * 2);
				dust.noGravity = true;
			}
		}
	}
}
