using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class Mossvine : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;

		DustType = DustID.Grass;

		AddMapEntry(new Color(54, 104, 55));
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		Tile below = Main.tile[i, j + 1];
		tile.TileFrameX = (short)(tile.TileFrameNumber * 18);

		if (!below.HasTile || below.TileType != Type)
		{
			tile.TileFrameY = (short)(Main.rand.Next(2) * 18 + 72);
		}
		else
		{
			tile.TileFrameY = (short)(Main.rand.Next(4) * 18);
		}

		return false;
	}
}
