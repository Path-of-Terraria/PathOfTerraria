using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class PurpleClouds : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(new Color(126, 108, 216));

		DustType = DustID.PurpleMoss;
	}
}
