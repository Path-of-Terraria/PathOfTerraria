using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

public static class RealtimeSteps
{
	public static RealtimeStep PlaceTile(int x, int y, int tile)
	{
		return new RealtimeStep((i, j) => WorldGen.PlaceTile(i, j, tile), new Point16(x, y));
	}

	public static RealtimeStep KillTile(int x, int y)
	{
		return new RealtimeStep((i, j) =>
		{
			WorldGen.KillTile(i, j);
			return !Main.tile[i, j].HasTile;
		}, new Point16(x, y));
	}

	public static RealtimeStep SmoothSlope(int x, int y, bool quickSkip = false)
	{
		return new RealtimeStep((i, j) =>
		{
			Tile.SmoothSlope(i, j);
			return !quickSkip;
		}, new Point16(x, y));
	}

	public static RealtimeStep SetWall(int x, int y, int wall, bool quickSkip = false)
	{
		return new RealtimeStep((i, j) =>
		{
			WorldGen.PlaceWall(i, j, wall);
			return !quickSkip || Main.tile[i, j].WallType == wall;
		}, new Point16(x, y));
	}
}
