using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class FrayedRope : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;

		DustType = DustID.Rope;

		AddMapEntry(new Color(183, 163, 152));
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		j++;

		while (Main.tile[i, j].TileType == TileID.Rope || Main.tile[i, j].TileType == ModContent.TileType<VoodooRope>())
		{
			WorldGen.KillTile(i, j, fail, effectOnly, true);
			j++;
		}
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		tile.TileFrameX = 0;
		tile.TileFrameY = 0;

		return false;
	}
}