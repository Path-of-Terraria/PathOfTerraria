using PathOfTerraria.Common.Tiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class MossyPurpleClouds : ModTile, IAutoloadTileItem
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(new Color(26, 63, 31));

		DustType = DustID.PurpleMoss;
	}
}
