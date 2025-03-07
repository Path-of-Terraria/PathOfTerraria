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
	}

	public static void MutualMerge(int type, int other)
	{
		Main.tileMerge[type][other] = true;
		Main.tileMerge[other][type] = true;
	}
}
