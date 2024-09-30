using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class RoyalHoneyClumpTile : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileFrameImportant[Type] = true;

		DustType = DustID.Honey;

		AddMapEntry(new Color(255, 156, 12));
	}

	public override void HitWire(int i, int j)
	{
		Wiring.SkipWire(i, j);
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameY == 0)
		{
			tile.TileFrameY = 0;
		}
	}
}