using Terraria.ID;

namespace PathOfTerraria.Common.Tiles;

internal class TileMerger : ModSystem
{
	public override void SetStaticDefaults()
	{
		MutualMerge(TileID.TinPlating, TileID.TinBrick);
		MutualMerge(TileID.CopperPlating, TileID.TinBrick);

		Main.tileBrick[TileID.TinPlating] = true;
		Main.tileBrick[TileID.CopperPlating] = true;
		Main.tileBrick[TileID.SandstoneBrick] = true;
		Main.tileBrick[TileID.Obsidian] = true;
		Main.tileBrick[TileID.SandStoneSlab] = true;
		Main.tileBrick[TileID.Sandstone] = true;
	}

	public static void MutualMerge(int type, int other)
	{
		Main.tileMerge[type][other] = true;
		Main.tileMerge[other][type] = true;
	}
}
