namespace PathOfTerraria.Common.Tiles;

internal static class TileExtensions
{
	public static Vector2 DrawPosition(int i, int j)
	{
		Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) - Main.screenPosition + offScreen;
		return position;
	}

	public static Rectangle BasicFrame(int i, int j)
	{
		return Main.tile[i, j].BasicFrame();
	}

	public static Rectangle BasicFrame(this Tile tile)
	{
		return new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
	}
}
