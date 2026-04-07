using PathOfTerraria.Common.Tiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class CypressWood : ModTile, IAutoloadTileItem
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlendAll[Type] = true;
		Main.tileBlockLight[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;

		AddMapEntry(new Color(121, 104, 73));

		DustType = DustID.RichMahogany;
	}
}
