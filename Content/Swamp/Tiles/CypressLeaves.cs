using PathOfTerraria.Common.Tiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class CypressLeaves : ModTile, IAutoloadTileItem
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlendAll[Type] = true;
		Main.tileBlockLight[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;

		AddMapEntry(new Color(79, 116, 23));

		DustType = DustID.JungleGrass;
	}
}
