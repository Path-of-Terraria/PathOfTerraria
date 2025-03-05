using Terraria.ID;

namespace PathOfTerraria.Common.Tiles;

internal class TinBrickMerger : ModSystem
{
	public override void SetStaticDefaults()
	{
		Main.tileMerge[TileID.TinPlating][TileID.TinBrick] = true;
		Main.tileMerge[TileID.TinBrick][TileID.TinPlating] = true;
		Main.tileMerge[TileID.CopperPlating][TileID.TinBrick] = true;
		Main.tileMerge[TileID.TinBrick][TileID.CopperPlating] = true;
		Main.tileBrick[TileID.TinPlating] = true;
		Main.tileBrick[TileID.CopperPlating] = true;
	}
}
